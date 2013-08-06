using System;
using System.Data;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Mictlanix.MySuite.Client.Data;
using Mictlanix.CFDv32;

namespace Mictlanix.MySuite.Client
{
	public class MySuiteClient
	{
		public static string DEVELOPMENT_URL = @"https://www.mysuitetest.com/mx.com.fact.wsFront/FactWSFront.asmx";
		public static string PRODUCTION_URL = @"https://www.mysuitecfdi.com/mx.com.fact.wsFront/FactWSFront.asmx";
		static string COUNTRY_CODE = "MX";
		static string OUTPUT_FORMAT = "XML";

		static readonly BasicHttpBinding binding = new BasicHttpBinding (BasicHttpSecurityMode.Transport) {
			MaxBufferPoolSize = int.MaxValue,
			MaxReceivedMessageSize = int.MaxValue,
			ReaderQuotas = new XmlDictionaryReaderQuotas
            {
                MaxDepth = int.MaxValue,
                MaxStringContentLength = int.MaxValue,
                MaxArrayLength = int.MaxValue,
                MaxBytesPerRead = int.MaxValue,
                MaxNameTableCharCount = int.MaxValue,
            }
		};

		DocID last_identifier;
		string url_end_point;
		EndpointAddress address;
		string[] output_data = new string[3];

		public MySuiteClient (string requestor, string entity, string loginName)
            : this(requestor, entity, loginName, PRODUCTION_URL)
		{
		}

		public MySuiteClient (string requestor, string entity, string loginName, string urlEndPoint)
		{
			Requestor = requestor;
			Entity = entity;
			LoginName = loginName;
			UrlEndPoint = urlEndPoint;

			ServicePointManager.ServerCertificateValidationCallback = 
				(object sp, X509Certificate c, X509Chain r, SslPolicyErrors e) => true;
		}

		public string Requestor { get; private set; }

		public string Entity { get; private set; }

		public string User { get { return Requestor; } }

		public string LoginName { get; private set; }

		public string UserName {
			get { return string.Format ("{0}.{1}.{2}", COUNTRY_CODE, Entity, LoginName); }
		}

		public DocID LastIdentifier {
			get { return last_identifier; }
		}

		public string UrlEndPoint {
			get { return url_end_point;}
			set {
				if (url_end_point == value)
					return;

				url_end_point = value;
				address = new EndpointAddress (url_end_point);
			}
		}

		public Comprobante Stamp (TFactDocMX doc)
		{
			Reset ();

			var ws = new MySuiteWSClient (binding, address);
			var result = ws.RequestTransaction (Requestor, Transactions.CONVERT_NATIVE_XML.ToString (),
			                                    COUNTRY_CODE, Entity, User, UserName,
			                                    doc.ToXmlString (), OUTPUT_FORMAT, "");
			ws.Close ();

			if (!result.Response.Result) {
				throw new MySuiteClientException (result.Response.Code,
				                                  result.Response.Description,
				                                  result.Response.Hint,
				                                  result.Response.Data);
			}

			using (var ms = new MemoryStream (DecodeFromBase64 (result.ResponseData.ResponseData1))) {
				var cfd = Comprobante.FromXml (ms);
				cfd.Addenda = null;
				return cfd;
			}
		}

		public MySuiteResult CreateDocument (TFactDocMX doc)
		{
			Reset ();

			var ws = new MySuiteWSClient (binding, address);
			var result = ws.RequestTransaction (Requestor, Transactions.CONVERT_NATIVE_XML.ToString (),
	                                            COUNTRY_CODE, Entity, User, UserName,
			                                    doc.ToXmlString (), OUTPUT_FORMAT, "");
			ws.Close ();

			if (result.Response.Result)
				SaveData (result.ResponseData);

			return HandleResponse (result.Response);
		}

		public MySuiteResult GetDocument (string branch, string serial)
		{
			Reset ();

			var ws = new MySuiteWSClient (binding, address);
			var result = ws.RequestTransaction (Requestor, Transactions.GET_DOCUMENT.ToString (),
			                                    COUNTRY_CODE, Entity, User, UserName,
			                                    branch, serial, OUTPUT_FORMAT);
			ws.Close ();

			if (result.Response.Result)
				SaveData (result.ResponseData);

			return HandleResponse (result.Response);
		}

		public MySuiteResult CancelDocument (string batch, string serial)
		{
			Reset ();

			var ws = new MySuiteWSClient (binding, address);
			var result = ws.RequestTransaction (Requestor, Transactions.CANCEL_DOCUMENT.ToString (),
			                                    COUNTRY_CODE, Entity, User, UserName,
			                                    batch, serial, string.Empty);
			ws.Close ();

			return HandleResponse (result.Response);
		}

		MySuiteResult HandleResponse (FactResponse response)
		{
			MySuiteResult doc_id;

			if (!response.Result) {
				throw new MySuiteClientException (response.Code,
	                                              response.Description,
	                                              response.Hint,
	                                              response.Data);
			}

			last_identifier = response.Identifier;

			doc_id = new MySuiteResult {
				Batch = response.Identifier.Batch,
				Serial = response.Identifier.Serial,
				Date = DateTime.Parse (response.Identifier.IssuedTimeStamp)
			};

			return doc_id;
		}

		public string GetData ()
		{
			return UTF8Encoding.UTF8.GetString(DecodeFromBase64 (output_data [0]));
		}

		void SaveData (FactResponseData data)
		{
			output_data [0] = data.ResponseData1;
			//output_data [1] = data.ResponseData2;
			//output_data [2] = data.ResponseData3;
		}

		void Reset ()
		{
			for (int i = 0; i < 3; i++)
				output_data [i] = null;

			last_identifier = null;
		}

		static public byte[] DecodeFromBase64 (string encodedData)
		{
			return System.Convert.FromBase64String (encodedData);
		}

		static TFactDocMX Convert (Comprobante cfd)
		{
			TFactDocMX doc = new TFactDocMX ();

			doc.Identificacion = new TFactDocMXIdentificacion {
				CdgPaisEmisor = TSenderCountryCode.MX,
				RFCEmisor = cfd.Emisor.rfc,
				RazonSocialEmisor = cfd.Emisor.nombre,
				LugarExpedicion = cfd.LugarExpedicion,
				NumCtaPago = cfd.NumCtaPago,
				Usuario = "1"
			};

			switch (cfd.tipoDeComprobante) {
			case ComprobanteTipoDeComprobante.ingreso:
				doc.Identificacion.TipoDeComprobante = TTipoDeDocumento.FACTURA;
				break;
			case ComprobanteTipoDeComprobante.egreso:
				doc.Identificacion.TipoDeComprobante = TTipoDeDocumento.NOTA_DE_CREDITO;
				break;
			case ComprobanteTipoDeComprobante.traslado:
				doc.Identificacion.TipoDeComprobante = TTipoDeDocumento.CARTA_PORTE;
				break;
			}

			doc.Emisor = new TFactDocMXEmisor {
				DomicilioFiscal = (cfd.Emisor.DomicilioFiscal == null) ? null : new TDomicilioMexicano {
					Calle = cfd.Emisor.DomicilioFiscal.calle,
					NumeroExterior = cfd.Emisor.DomicilioFiscal.noExterior,
					NumeroInterior = cfd.Emisor.DomicilioFiscal.noInterior,
					Colonia = cfd.Emisor.DomicilioFiscal.colonia,
					Localidad = cfd.Emisor.DomicilioFiscal.localidad,
					Municipio = cfd.Emisor.DomicilioFiscal.municipio,
					Estado = cfd.Emisor.DomicilioFiscal.estado,
					Pais = cfd.Emisor.DomicilioFiscal.pais,
					CodigoPostal = cfd.Emisor.DomicilioFiscal.codigoPostal
				}
			};

			doc.Emisor.RegimenFiscal = new string [cfd.Emisor.RegimenFiscal.Length];
			for (int i = 0; i < cfd.Emisor.RegimenFiscal.Length; i++) {
				doc.Emisor.RegimenFiscal [i] = cfd.Emisor.RegimenFiscal [i].Regimen;
			}

			doc.Receptor = new TFactDocMXReceptor {
				CdgPaisReceptor = TCountryCode.MX,
				RFCReceptor = cfd.Receptor.rfc,
				NombreReceptor = cfd.Receptor.nombre,
				Domicilio = (cfd.Receptor.Domicilio == null) ? null : new TFactDocMXReceptorDomicilio {
					Item = new TDomicilioMexicano {
						Calle = cfd.Receptor.Domicilio.calle,
						NumeroExterior = cfd.Receptor.Domicilio.noExterior,
						NumeroInterior = cfd.Receptor.Domicilio.noInterior,
						Colonia = cfd.Receptor.Domicilio.colonia,
						Localidad = cfd.Receptor.Domicilio.localidad,
						Municipio = cfd.Receptor.Domicilio.municipio,
						Estado = cfd.Receptor.Domicilio.estado,
						Pais = cfd.Receptor.Domicilio.pais,
						CodigoPostal = cfd.Receptor.Domicilio.codigoPostal
					}
				}
			};

			doc.ComprobanteEx = new TComprobanteEx {
				TerminosDePago = new TComprobanteExTerminosDePago {
					CondicionesDePago = cfd.condicionesDePago,
					MetodoDePago = cfd.metodoDePago
				}
			};

			doc.Conceptos = new TFactDocMXConcepto [cfd.Conceptos.Length];

			for (int i = 0; i < cfd.Conceptos.Length; i++) {
				var item = cfd.Conceptos [i];

				doc.Conceptos[i] = new TFactDocMXConcepto {
					Cantidad = item.cantidad,
					UnidadDeMedida = item.unidad,
					Descripcion = item.descripcion,
					ValorUnitario = new TNonNegativeAmount {
						Value = item.valorUnitario
					},
					Importe = new TNonNegativeAmount {
						Value = item.importe
					},
					// FIXME: there's no tax info in ComprobanteConcepto
					/* ConceptoEx = new TConceptoEx {
						Impuestos = new TTax[] {
							new TTax {
								Contexto = TTaxContext.FEDERAL,
								Operacion = TTaxOperation.TRASLADO,
								Codigo = "IVA",
								Base = new TNonNegativeAmount {
									Value = item.importe
								},
								Tasa = 0, 				
								Monto = new TNonNegativeAmount {
									Value = 0
								}
							}
						}
					}*/
				};
			}

			doc.Totales = new TFactDocMXTotales {
				Moneda = (TCurrencyCode)Enum.Parse (typeof(TCurrencyCode), cfd.Moneda),
				TipoDeCambioVenta = decimal.Parse (cfd.TipoCambio),
				SubTotalBruto = new TNonNegativeAmount {
					Value = doc.Conceptos.Sum (x => x.Importe.Value)
				},
				SubTotal = new TNonNegativeAmount {
					Value = doc.Conceptos.Sum (x => x.Importe.Value)
				},
				ResumenDeDescuentosYRecargos = new TResumenDeDescuentosYRecargos {
					TotalDescuentos = new TNonNegativeAmount { Value = 0m },
					TotalRecargos = new TNonNegativeAmount { Value = 0m }
				},
				ResumenDeImpuestos = new TResumenDeImpuestos {
					TotalTrasladosLocales = new TNonNegativeAmount {
						Value = 0
					},
					TotalRetencionesLocales = new TNonNegativeAmount {
						Value = 0
					},
					TotalIVATrasladado = new TNonNegativeAmount {
						Value = doc.Conceptos.Sum (x => x.ConceptoEx.Impuestos == null ? 0m : x.ConceptoEx.Impuestos.Sum(y => y.Monto.Value))
					},
					TotalIEPSTrasladado = new TNonNegativeAmount {
						Value = 0
					},
					TotalTrasladosFederales = new TNonNegativeAmount {
						Value = doc.Conceptos.Sum (x => x.ConceptoEx.Impuestos == null ? 0m : x.ConceptoEx.Impuestos.Sum(y => y.Monto.Value))
					},
					TotalISRRetenido = new TNonNegativeAmount {
						Value = 0
					},
					TotalIVARetenido = new TNonNegativeAmount {
						Value = 0
					},
					TotalRetencionesFederales = new TNonNegativeAmount {
						Value = 0
					}
				},
				FormaDePago = cfd.formaDePago
			};

			if (doc.Conceptos.Count (x => x.ConceptoEx.Impuestos != null) > 0) {
				doc.Totales.Impuestos = new TTax[] {
					new TTax {
						Contexto = TTaxContext.FEDERAL,
						Operacion = TTaxOperation.TRASLADO,
						Codigo = "IVA",
						Base = new TNonNegativeAmount {
							Value = doc.Conceptos.Sum (x => x.ConceptoEx.Impuestos == null ? 0m : x.ConceptoEx.Impuestos.Sum(y => y.Base.Value))
						},
						Tasa = doc.Conceptos.Max (x => x.ConceptoEx.Impuestos == null ? 0m : x.ConceptoEx.Impuestos.Max(y => y.Tasa)),
						Monto = new TNonNegativeAmount {
							Value = doc.Conceptos.Sum (x => x.ConceptoEx.Impuestos == null ? 0m : x.ConceptoEx.Impuestos.Sum(y => y.Monto.Value))
						}
					}
				};
			}

			doc.Totales.Total = new TNonNegativeAmount {
				Value = doc.Totales.SubTotal.Value + doc.Totales.ResumenDeImpuestos.TotalIVATrasladado.Value
			};
			doc.Totales.TotalEnLetra = "";

			return doc;
		}

	}
}

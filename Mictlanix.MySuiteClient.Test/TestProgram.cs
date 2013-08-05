using System;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Mictlanix.MySuite.Client;
using Mictlanix.MySuite.Client.Data;

namespace Mictlanix.MySuite.Client.Test
{
	class TestProgram
	{
		public static void Main(string[] args)
		{
			MySuiteClient cli;

			cli = new MySuiteClient ("12211111-1111-1111-1111-111111111111",
			                         "AAA010101AAA", "FISCALDOM", MySuiteCountryCode.MX,
			                         MySuiteClient.DEVELOPMENT_URL);

			TestCreate (cli);
			//TestGet(cli);
			//TestSearch(cli);
			//TestCancel(cli);

			Console.ReadLine();
		}

		// CONVERT_NATIVE_XM
		public static void TestCreate (MySuiteClient cli)
		{
			MySuiteDocId result;

			result = cli.CreateDocument (CFDAsObject (), MySuiteOutputFormat.XML | MySuiteOutputFormat.PDF);
			Console.WriteLine ("Result: {0}, {1}, {2}", result.Batch, result.Serial, result.Date);
			cli.SaveFile(string.Format("CFD-{0}-{1}", result.Batch, result.Serial), MySuiteOutputFormat.XML | MySuiteOutputFormat.PDF);
		}

		// GET_DOCUMENT
		public static void TestGet (MySuiteClient cli)
		{
			MySuiteDocId result;

			result = cli.GetDocument("FISCALDOM", "55963", MySuiteOutputFormat.XML);
			Console.WriteLine("Result: {0}, {1}, {2}", result.Batch, result.Serial, result.Date);
			cli.SaveFile(string.Format("CFD-{0}-{1}", result.Batch, result.Serial), MySuiteOutputFormat.XML);
		}

		// CANCEL_XML
		public static void TestCancel (MySuiteClient cli)
		{
			MySuiteDocId result;

			result = cli.CancelDocument ("A", "4");

			Console.WriteLine ("Result: {0}, {1}, {2}", result.Batch, result.Serial, result.Date);
		}
		
		public static TFactDocMX CFDAsObject()
		{
			TFactDocMX obj = new TFactDocMX();

			obj.Identificacion = new TFactDocMXIdentificacion {
				CdgPaisEmisor = TSenderCountryCode.MX,
				TipoDeComprobante = TTipoDeDocumento.FACTURA,
				RFCEmisor = "AAA010101AAA",
				RazonSocialEmisor = "EMPRESA EMISORA DE PRUEBA",
				Usuario = "1",
				NumeroInterno = "000001",
				LugarExpedicion = "México, DF"
			};

			obj.Emisor = new TFactDocMXEmisor {
				RegimenFiscal = new [] {
					"Régimen de las Personas Físicas con Actividades Empresariales y Profesionales"
				},
				DomicilioFiscal = null
			};

			obj.Receptor = new TFactDocMXReceptor {
				CdgPaisReceptor = TCountryCode.MX,
				RFCReceptor = "BBB010101BBA",
				NombreReceptor = "EMPRESA RECEPTORA DE PRUEBA",
				Domicilio = null
			};

            obj.Conceptos = new TFactDocMXConcepto[] {
                new TFactDocMXConcepto {
                    Cantidad = 10,
                    UnidadDeMedida = "LITRO",
                    Descripcion = "GASOLINA MAGNA",
					ValorUnitario = new TNonNegativeAmount { Value = 7.241379m },
					Importe = new TNonNegativeAmount { Value = 72.413793m },
                    ConceptoEx = new TConceptoEx {
                        Impuestos = new TTax[] {
                            new TTax {
                                Contexto = TTaxContext.FEDERAL,
                                Operacion = TTaxOperation.TRASLADO,
                                Codigo = "IVA",
								Base = new TNonNegativeAmount { Value = 72.413793m },
								Tasa = 16m,
								Monto = new TNonNegativeAmount { Value =  11.586207m }
                            }
                        },
                        FechaDeCaducidad = new DateTime(2011, 12, 31),
                        FechaDeCaducidadSpecified = true,
                        NumeroDeLote = "LOTE123"
                    }
                }
            };

            obj.Totales = new TFactDocMXTotales
            {
                Moneda = TCurrencyCode.MXN,
                TipoDeCambioVenta = 1m,
				SubTotalBruto = new TNonNegativeAmount { Value = 72.413793m },
				SubTotal = new TNonNegativeAmount { Value = 72.413793m },
                ResumenDeDescuentosYRecargos = new TResumenDeDescuentosYRecargos {
					TotalDescuentos = new TNonNegativeAmount { Value = 0m },
					TotalRecargos = new TNonNegativeAmount { Value = 0m }
                },
                Impuestos = new TTax[] {
                    new TTax {
                        Contexto = TTaxContext.FEDERAL,
                        Operacion = TTaxOperation.TRASLADO,
                        Codigo = "IVA",
						Base = new TNonNegativeAmount { Value = 72.413793m },
                        Tasa = 16m,
						Monto = new TNonNegativeAmount { Value = 11.586207m }
                    }
                },
                ResumenDeImpuestos = new TResumenDeImpuestos {
					TotalTrasladosFederales = new TNonNegativeAmount { Value = 11.586207m },
					TotalIVATrasladado = new TNonNegativeAmount { Value = 11.586207m },
					TotalIEPSTrasladado = new TNonNegativeAmount { Value = 0 },
					TotalRetencionesFederales = new TNonNegativeAmount { Value = 0m },
					TotalISRRetenido = new TNonNegativeAmount { Value =  0 },
					TotalIVARetenido = new TNonNegativeAmount { Value = 0 },
					TotalTrasladosLocales = new TNonNegativeAmount { Value = 3.6m },
					TotalRetencionesLocales = new TNonNegativeAmount { Value = 0m }
                },
				Total = new TNonNegativeAmount { Value = 87.6m },
                TotalEnLetra = "OCHENTA Y SIETE PESOS 60 /100 M.N.",
                FormaDePago = "PAGO EN UNA SOLA EXHIBICION"
            };

			obj.ComprobanteEx = new TComprobanteEx
			{
				/*DatosDeNegocio = new TComprobanteExDatosDeNegocio {
					Sucursal = "FISCALDOM"
				},*/
				TerminosDePago = new TComprobanteExTerminosDePago
				{
					//MedioDePago = "Efectivo",
					//CondicionesDePago = "CONTADO",
					MetodoDePago = "Efectivo"
				}
			};

			return obj;
		}

	}
}


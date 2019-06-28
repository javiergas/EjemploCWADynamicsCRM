using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace ListaPrecioALineaContrato
{
    public class ListaPrecioALineaContrato : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Get a reference to the Organization service.
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            tracingService.Trace("Comenzando la ejecución");

            if (context.InputParameters != null)
            {
                #region Lista_de_precios_para_pedidos

                Entity entity = (Entity)context.InputParameters["Target"];

                //foreach (KeyValuePair<String, Object> attribute in entity.Attributes)
                //{
                //    tracingService.Trace(attribute.Key + ": " + attribute.Value);
                //}

                tracingService.Trace("Contractdetailid: " + entity.Id);

                if ((entity.Attributes.Contains("productid") == false) || (entity["productid"] == null))
                {
                    tracingService.Trace("Sin productid");
                    entity.Attributes.Add("new_listadepreciosparapedidos", null);
                    return;
                }

                EntityReference producto_asociado = (EntityReference)entity.Attributes["productid"];

                ColumnSet Columnas = new ColumnSet(new String[] { "new_listadepreciosparapedidos", "new_totaldecasos" });
                Entity producto = service.Retrieve("product", producto_asociado.Id, Columnas);

                if (producto.Attributes.Contains("new_totaldecasos"))
                {
                    tracingService.Trace("Agregar Total de casos desde producto");
                    entity.Attributes["totalallotments"] = producto.Attributes["new_totaldecasos"];
                    entity.Attributes["allotmentsremaining"] = producto.Attributes["new_totaldecasos"];
                }
                else
                {
                    tracingService.Trace("Sin total de casos en el producto");
                    entity.Attributes["totalallotments"] = "9998";
                }

                if ((producto.Attributes.Contains("new_listadepreciosparapedidos") == false) || (producto["new_listadepreciosparapedidos"] == null))
                {
                    tracingService.Trace("Sin new_listadepreciosparapedidos");
                    entity.Attributes.Add("new_listadepreciosparapedidos", null);
                    return;
                }

                EntityReference lista_precios = (EntityReference)(producto.Attributes["new_listadepreciosparapedidos"]);
                if (entity.Attributes.Contains("new_listadepreciosparapedidos"))
                {
                    entity.Attributes["new_listadepreciosparapedidos"] = lista_precios;
                }
                else
                {
                    entity.Attributes.Add("new_listadepreciosparapedidos", lista_precios);
                }

                #endregion

                tracingService.Trace("Message: " + context.MessageName);

                if (context.MessageName == "Create")
                {
                    //foreach (KeyValuePair<String, Object> attribute in entity.Attributes)
                    //{
                    //    tracingService.Trace(attribute.Key + ": " + attribute.Value);
                    //}
                    #region contrato-actual-y-original
                    if (entity.Attributes.Contains("new_lineadecontratoactual"))
                    {
                        EntityReference lineacontratoactual = (EntityReference)entity.Attributes["new_lineadecontratoactual"];
                        //identificar contato original
                        if (entity.Attributes.Contains("new_lineadecontratooriginal"))
                        {
                            entity.Attributes["new_lineadecontratooriginal"] = lineacontratoactual;
                        }
                        else
                        {
                            entity.Attributes.Add("new_lineadecontratooriginal", lineacontratoactual);
                        }

                        //registrar linea de contracto actual en el atributo 
                        entity.Attributes["new_lineadecontratoactual"] = new EntityReference("contractdetail", entity.Id);
                    }
                    else
                    {
                        //registrar linea de contrato actual en nuevo atributo
                        entity.Attributes.Add("new_lineadecontratoactual", new EntityReference("contractdetail", entity.Id));
                    }

                    tracingService.Trace("3");
                    #endregion
                }
            }
        }
    }
}
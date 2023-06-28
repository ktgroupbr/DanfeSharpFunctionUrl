using System.Net;
using DanfeSharp.Modelo;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DanfeSharpFunctionUrl
{
    public class DanfeFunction
    {
        private readonly ILogger _logger;

        public DanfeFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DanfeFunction>();
        }

        [Function("view")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

             Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("pt-BR");
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

            var modelo = DanfeViewModelCreator.CriarDeArquivoXml(req.Body);
            modelo.Orientacao = DanfeSharp.Orientacao.Retrato;

            var filepdf = System.IO.Path.GetTempPath() + Path.GetRandomFileName() + ".pdf";

            using (DanfeSharp.Danfe danfe = new DanfeSharp.Danfe(modelo))
            {
               
                danfe.Gerar();
                danfe.Salvar(filepdf);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "pdf/application; charset=utf-8");
            response.WriteBytes(File.ReadAllBytes(filepdf));

            if (File.Exists(filepdf))
                File.Delete(filepdf);

            return response;
        }
    }
}

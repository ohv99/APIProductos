using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using APIProductos.Models;
using Microsoft.AspNetCore.Cors;
namespace APIProductos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentoController : ControllerBase
    {
        //variables para llamar la conexion y ruta donde se guardaran los archivos
        private readonly string _DefaultConnection;
        private readonly string _RutaServidor;
        
        //constructor para llamar las ruta de conexion 
        public DocumentoController(IConfiguration config)
        {
            _DefaultConnection = config.GetConnectionString("DefaultConnection");
            _RutaServidor = config.GetSection("Configuracion").GetSection("RutaServidor").Value;
        }
        //--------------------------------------------------------------------------------------
        [HttpPost]
        [Route("Subir")]
        //quitar limite de bytes
        [DisableRequestSizeLimit,RequestFormLimits(MultipartBodyLengthLimit =int.MaxValue,ValueLengthLimit=int.MaxValue)]
        public IActionResult Subir([FromForm] Documento request)
        {
            //subir el archivo
            string rutaDocumento = Path.Combine(_RutaServidor, request.Archivo.FileName);
            try
            {
                using(FileStream newFile = System.IO.File.Create(rutaDocumento))
                {
                    request.Archivo.CopyTo(newFile);
                    newFile.Flush();
                }
                //guardarlo en la base de datos
                using (var conexion = new SqlConnection(_DefaultConnection))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("sp_guardar_documento", conexion);
                    cmd.Parameters.AddWithValue("descripcion", request.Descripcion);
                    cmd.Parameters.AddWithValue("ruta",rutaDocumento);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.ExecuteNonQuery();
                }

                return StatusCode(StatusCodes.Status200OK, new { mensaje = "Documento Guardado" });

            }
            catch(Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }

        }
        

    }
}


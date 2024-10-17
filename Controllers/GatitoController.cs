using AdoptaMascota.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

namespace AdoptaMascota.Controllers
{
    public class GatitoController : Controller
    {
        public readonly IConfiguration _config;

        public GatitoController(IConfiguration IConfig)
        {
            _config = IConfig;
        }
        public IActionResult Index()
        {
            return View();
        }
        IEnumerable<Gatito> Gatos()
        {
            List<Gatito> gat = new List<Gatito>();
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cn"]))
            {
                SqlCommand cmd = new SqlCommand("sp_listar_gato", cn);
                //Aperturamos la base de datos
                cn.Open();
                //realizamos la respectiva ejecucion....
                SqlDataReader dr = cmd.ExecuteReader();
                //aplicamos un bucle while  
                while (dr.Read())
                {
                    gat.Add(new Gatito
                    {
                        //recuperamos lo que viene en la base de datos
                        //y almacenamos en las propiedades
                        idGato = dr.GetInt32(0),
                        nombre = dr.GetString(1),
                        edad = dr.GetInt32(2),
                        sexo = dr.GetString(3),
                        Color = dr.GetString(4),
                        adoptado = dr.GetString(5),
                    });

                }//Fin while
            }//fin using
            //retornamos e llistado
            return gat;

        }//fin del metodo IEnumerable

        public async Task<IActionResult> ListadoGatitos(int g)
        {
            int nr = 5;
            int tr = Gatos().Count();
            int paginas = tr > 0 ? (tr % nr == 0 ? tr / nr : tr / nr + 1) : 0;
            ViewBag.paginas = paginas;
            return View(await Task.Run(() => Gatos().Skip(g * nr).Take(nr)));
        }
        //retorna la lista de autos
        public async Task<IActionResult> Listado()
        {
            //retornamos al a vista
            return View(await Task.Run(() => Gatos()));
        }
        //Codigo para registrar autos
        //Listado  para cargar el select de marca
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            Gatito gato = new Gatito();
            return View(gato);
        }//Fin del metodo create

        [HttpPost]
        public async Task<IActionResult> Create(Gatito model)
        {
            String mensaje = "";
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cn"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("sp_registrar_gato", cn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                //agregamos los parametros..
                cmd.Parameters.AddWithValue("@nom", model.nombre);
                cmd.Parameters.AddWithValue("@edad", model.edad);
                cmd.Parameters.AddWithValue("@sex", model.sexo);
                cmd.Parameters.AddWithValue("@col", model.Color);
                cmd.Parameters.AddWithValue("@adop", model.adoptado);
                //realizamos la ejecucion...
                int c = cmd.ExecuteNonQuery();
                mensaje = $"Registro insertado{c} de gato";
            }//fin del using
            ViewBag.mensaje = mensaje;
            //redirecciones
            return RedirectToAction("ListadoGatitos", "Gatito");
        }
        Gatito Buscar(int id)
        {
            Gatito? reg = Gatos().Where(v => v.idGato == id).FirstOrDefault();
            return reg;
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Gatito reg = Buscar(id);
            //aplicamos una condicion
            if (reg == null) return RedirectToAction("ListadoGatitos");
            return View(reg);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Gatito model)
        {
            string mensaje = "";
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cn"]))
            {
                SqlCommand cmd = new SqlCommand("sp_actualizar_gato", cn);
                cn.Open();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idcat", model.idGato);
                cmd.Parameters.AddWithValue("@nom", model.nombre);
                cmd.Parameters.AddWithValue("@edad", model.edad);
                cmd.Parameters.AddWithValue("@sex", model.sexo);
                cmd.Parameters.AddWithValue("@col", model.Color);
                cmd.Parameters.AddWithValue("@adop", model.adoptado);
                int c = cmd.ExecuteNonQuery();
                mensaje = $"registro actualizado{c} gato";
            }
            ViewBag.mensaje = mensaje;
            return RedirectToAction("ListadoGatitos", "Gatito");
        }
        [HttpGet, ActionName("Delete")]
        public IActionResult Delete(int id)
        {
            string mensaje = "";
            //obtenemos la conexion
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cn"]))
            {
                SqlCommand cmd = new SqlCommand("sp_eliminar_gato", cn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                //abrimos la base de datos...
                cn.Open();
                //agregamos el parametro...
                cmd.Parameters.AddWithValue("@idcat ", id);
                //realizamos la ejecucion del procedimiento almacenado...
                int c = cmd.ExecuteNonQuery();
                mensaje = $"registro eliminado{c} de la Gato";
            }//fin del using

            //enviamos mensaje hacia la listado
            ViewBag.mensaje = mensaje;
            //redireccionamos hacia el listado
            return RedirectToAction("ListadoGatitos");
        }//fin del metodo POST

    }
}


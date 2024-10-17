using AdoptaMascota.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AdoptaMascota.Controllers
{
    public class PerritoController : Controller
    {
        public readonly IConfiguration _config;

        public PerritoController(IConfiguration IConfig)
        {
            _config = IConfig;
        }
        public IActionResult Index()
        {
            return View();
        }
        IEnumerable<Perrito> Perros()
        {
            List<Perrito> gat = new List<Perrito>();
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cn"]))
            {
                SqlCommand cmd = new SqlCommand("sp_listar_perro", cn);
                //Aperturamos la base de datos
                cn.Open();
                //realizamos la respectiva ejecucion....
                SqlDataReader dr = cmd.ExecuteReader();
                //aplicamos un bucle while  
                while (dr.Read())
                {
                    gat.Add(new Perrito
                    {
                        //recuperamos lo que viene en la base de datos
                        //y almacenamos en las propiedades
                        idPerro = dr.GetInt32(0),
                        nombre = dr.GetString(1),
                        edad = dr.GetInt32(2),
                        sexo = dr.GetString(3),
                        color = dr.GetString(4),
                        raza = dr.GetString(5),
                        adoptado = dr.GetString(6),
                    });

                }//Fin while
            }//fin using
            //retornamos e llistado
            return gat;

        }//fin del metodo IEnumerable

        public async Task<IActionResult> ListadoPerritos(int p)
        {
            int nr = 5;
            int tr = Perros().Count();
            int paginas = tr > 0 ? (tr % nr == 0 ? tr / nr : tr / nr + 1) : 0;
            ViewBag.paginas = paginas;
            return View(await Task.Run(() => Perros().Skip(p * nr).Take(nr)));
        }


        //retorna la lista de autos
        public async Task<IActionResult> Listado()
        {
            //retornamos al a vista
            return View(await Task.Run(() => Perros()));
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            Perrito perro = new Perrito();
            return View(perro);
        }//Fin del metodo create

        [HttpPost]
        public async Task<IActionResult> Create(Perrito model)
        {
            String mensaje = "";
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cn"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("sp_registrar_perro", cn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                //agregamos los parametros..
                cmd.Parameters.AddWithValue("@nom", model.nombre);
                cmd.Parameters.AddWithValue("@edad", model.edad);
                cmd.Parameters.AddWithValue("@sex", model.sexo);
                cmd.Parameters.AddWithValue("@col", model.color);
                cmd.Parameters.AddWithValue("@raz", model.raza);
                cmd.Parameters.AddWithValue("@adop", model.adoptado);
                //realizamos la ejecucion...
                int c = cmd.ExecuteNonQuery();
                mensaje = $"Registro insertado{c} de perro";
            }//fin del using
            ViewBag.mensaje = mensaje;
            //redirecciones
            return RedirectToAction("ListadoPerritos", "Perrito");
        }
        Perrito Buscar(int id)
        {
            Perrito? reg = Perros().Where(v => v.idPerro == id).FirstOrDefault();
            return reg;
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Perrito reg = Buscar(id);
            //aplicamos una condicion
            if (reg == null) return RedirectToAction("ListadoPerritos");
            return View(reg);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Perrito model)
        {
            string mensaje = "";
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cn"]))
            {
                SqlCommand cmd = new SqlCommand("sp_actualizar_perro", cn);
                cn.Open();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@iddog", model.idPerro);
                cmd.Parameters.AddWithValue("@nom", model.nombre);
                cmd.Parameters.AddWithValue("@edad", model.edad);
                cmd.Parameters.AddWithValue("@sex", model.sexo);
                cmd.Parameters.AddWithValue("@col", model.color);
                cmd.Parameters.AddWithValue("@raz", model.raza);
                cmd.Parameters.AddWithValue("@adop", model.adoptado);
                int c = cmd.ExecuteNonQuery();
                mensaje = $"registro actualizado{c} perro";
            }
            ViewBag.mensaje = mensaje;
            return RedirectToAction("ListadoPerritos", "Perrito");
        }
        [HttpGet, ActionName("Delete")]
        public IActionResult Delete(int id)
        {
            string mensaje = "";
            //obtenemos la conexion
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cn"]))
            {
                SqlCommand cmd = new SqlCommand("sp_eliminar_perro", cn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                //abrimos la base de datos...
                cn.Open();
                //agregamos el parametro...
                cmd.Parameters.AddWithValue("@iddog ", id);
                //realizamos la ejecucion del procedimiento almacenado...
                int c = cmd.ExecuteNonQuery();
                mensaje = $"registro eliminado{c} de la base de datos";
            }//fin del using

            //enviamos mensaje hacia la listado
            ViewBag.mensaje = mensaje;
            //redireccionamos hacia el listado
            return RedirectToAction("ListadoPerritos");
        }//fin del metodo POST
    }
}

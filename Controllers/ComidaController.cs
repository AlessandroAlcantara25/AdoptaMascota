using AdoptaMascota.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AdoptaMascota.Controllers
{
    public class ComidaController : Controller
    {

        public readonly IConfiguration _config;

        public ComidaController(IConfiguration IConfig)
        {
            _config = IConfig;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CalcuComida(Comida objcalcular)
        {
            decimal cantidad = objcalcular.cantidad;
            decimal precio = objcalcular.precio;
            decimal descuento = objcalcular.descuento;
            if ( objcalcular.cantidad >=10) {
                objcalcular.total = (int)Math.Ceiling(objcalcular.cantidad * objcalcular.precio / objcalcular.descuento);
            }
            return View(objcalcular);
        }

        IEnumerable<Comida> Comidas()
        {
            List<Comida> com = new List<Comida>();
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cn"]))
            {
                SqlCommand cmd = new SqlCommand("sp_listar_comida", cn);
                //Aperturamos la base de datos
                cn.Open();
                //realizamos la respectiva ejecucion....
                SqlDataReader dr = cmd.ExecuteReader();
                //aplicamos un bucle while  
                while (dr.Read())
                {
                    com.Add(new Comida
                    {
                        //recuperamos lo que viene en la base de datos
                        //y almacenamos en las propiedades
                        idComida = dr.GetInt32(0),
                        nombre = dr.GetString(1),
                        tipoComida = dr.GetString(2),
                        descuento = dr.GetDecimal(3),
                        animal = dr.GetString(4),
                        precio = dr.GetDecimal(5),
                    });

                }//Fin while
            }//fin using
            //retornamos e llistado
            return com;

        }//fin del metodo IEnumerable

        public async Task<IActionResult> ListadoComidas(int c)
        {
            int nr = 5;
            int tr = Comidas().Count();
            int paginas = tr > 0 ? (tr % nr == 0 ? tr / nr : tr / nr + 1) : 0;
            ViewBag.paginas = paginas;
            return View(await Task.Run(() => Comidas().Skip(c * nr).Take(nr)));
        }

        
        public async Task<IActionResult> Listado()
        {
            //retornamos al a vista
            return View(await Task.Run(() => Comidas()));
        }
        //Codigo para registrar autos
        //Listado  para cargar el select de marca
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            Comida comida = new Comida();
            return View(comida);
        }//Fin del metodo create

        [HttpPost]
        public async Task<IActionResult> Create(Comida model)
        {
            String mensaje = "";
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cn"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("sp_registrar_comida", cn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                //agregamos los parametros..
                cmd.Parameters.AddWithValue("@nom", model.nombre);
                cmd.Parameters.AddWithValue("@tcom", model.tipoComida);
                cmd.Parameters.AddWithValue("@des", model.descuento);
                cmd.Parameters.AddWithValue("@ani", model.animal);
                cmd.Parameters.AddWithValue("@pre", model.precio);
                //realizamos la ejecucion...
                int c = cmd.ExecuteNonQuery();
                mensaje = $"Registro insertado{c} de comida";
            }//fin del using
            ViewBag.mensaje = mensaje;
            //redirecciones
            return RedirectToAction("ListadoComidas", "Comida");
        }
        Comida Buscar(int id)
        {
            Comida? reg = Comidas().Where(c => c.idComida == id).FirstOrDefault();
            return reg;
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Comida reg = Buscar(id);
            //aplicamos una condicion
            if (reg == null) return RedirectToAction("ListadoComidas");
            return View(reg);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Comida model)
        {
            string mensaje = "";
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cn"]))
            {
                SqlCommand cmd = new SqlCommand("sp_actualizar_comida", cn);
                cn.Open();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idcom", model.idComida);
                cmd.Parameters.AddWithValue("@nom", model.nombre);
                cmd.Parameters.AddWithValue("@tcom", model.tipoComida);
                cmd.Parameters.AddWithValue("@des", model.descuento);
                cmd.Parameters.AddWithValue("@ani", model.animal);
                cmd.Parameters.AddWithValue("@pre", model.precio);
                int c = cmd.ExecuteNonQuery();
                mensaje = $"registro actualizado{c} de comida";
            }
            ViewBag.mensaje = mensaje;
            return RedirectToAction("ListadoComidas", "Comida");
        }
        [HttpGet, ActionName("Delete")]
        public IActionResult Delete(int id)
        {
            string mensaje = "";
            //obtenemos la conexion
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cn"]))
            {
                SqlCommand cmd = new SqlCommand("sp_eliminar_comida", cn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                //abrimos la base de datos...
                cn.Open();
                //agregamos el parametro...
                cmd.Parameters.AddWithValue("@idcom ", id);
                //realizamos la ejecucion del procedimiento almacenado...
                int c = cmd.ExecuteNonQuery();
                mensaje = $"registro eliminado{c} de Comida";
            }//fin del using

            //enviamos mensaje hacia la listado
            ViewBag.mensaje = mensaje;
            //redireccionamos hacia el listado
            return RedirectToAction("ListadoComidas");
        }//fin del metodo POST

    }
}


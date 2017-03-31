﻿using Newtonsoft.Json;
using Reinco.Entidades;
using Reinco.Recursos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Reinco.Interfaces.Obra
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AgregarObra : ContentPage, INotifyPropertyChanged
    {

        #region +---- Eventos ----+
        new public event PropertyChangedEventHandler PropertyChanged;
        #endregion



        #region +---- Atributos ----+
        private bool isRunning;
        int IdObra;
        WebService Servicio = new WebService();
        string Mensaje;
        public VentanaMensaje mensaje;
        #endregion



        #region +---- Constructores ----+
        public AgregarObra()
        {
            // Preparando la UI(Interfas de usuario)
            InitializeComponent();
            this.Title = "Crear Obra"; // nombre de la pagina

            // Servicios
            mensaje = new VentanaMensaje();

            // ObservableCollection
            propietarioItem = new ObservableCollection<PropietarioItem>();
            personalItem = new ObservableCollection<PersonalItem>();

            // Cargando las listas en los POP UPS
            CargarPropietarioItem();
            CargarPersonalItem();

            // Eventos Guardar Y Cancelar
            cancelar.Clicked += Cancelar_Clicked;
            guardar.Clicked += Guardar_Clicked;

            // Esstablecinedo el Contexto para poder usar lus bindings
            this.BindingContext = this;

        }

        public AgregarObra(int idObra, string Codigo, string Nombre)
        {
            // Preparando la UI(Interfas de usuario) MODIFICAR OBRA
            InitializeComponent();
            this.Title = Nombre; // nombre de la pagina
            nombre.Text = Nombre; // Lenando el campo Nombre Obra
            codigo.Text = Codigo; // llenando el campo Codigo Obra
            IdObra = Convert.ToInt16(idObra);

            asignarPropietario.Title = "Asigne un nuevo propietario"; // Titulo POP UPS Propietario
            asignarResponsable.Title = "Asigne un nuevo responsable"; // Titulo POP UPS Responsable
            guardar.Text = "Guardar Cambios"; // Cambiando el texto del Button Guargar a Guardar Cambios

            // Servicios
            mensaje = new VentanaMensaje();

            // ObservableCollection
            propietarioItem = new ObservableCollection<PropietarioItem>();
            personalItem = new ObservableCollection<PersonalItem>();

            // Cargando las listas en los POP UPS
            CargarPropietarioItem();
            CargarPersonalItem();


            // Eventos Guardar Y Cancelar
            //guardar.Clicked += modificarObra;
            //cancelar.Clicked += Cancelar_Clicked;

        } 
        #endregion



        #region +---- Propiedades ----+
        public bool IsRunning
        {
            set
            {
                if (isRunning != value)
                {
                    isRunning = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
                }
            }
            get
            {
                return isRunning;
            }
        }
        #endregion



        #region +---- ObservableCollection ----+
        public ObservableCollection<PropietarioItem> propietarioItem { get; set; } // Llamado desde un Binding BindablePicker ItemsSource="{Binding propietarioItem}" 
        public ObservableCollection<PersonalItem> personalItem { get; set; }  // Llamado desde un Binding BindablePicker ItemsSource="{Binding personalItem}" 
        #endregion



        #region +--------------- Cargando Usuarios Desde Web Service ---------------------------+
        private async void CargarPersonalItem()
        {
            try
            {
                dynamic usuarios = await Servicio.MetodoGet("ServicioUsuario.asmx", "MostrarUsuarios");
                personalItem.Clear();
                foreach (var item in usuarios)
                {
                    personalItem.Add(new PersonalItem
                    {
                        idUsuario = item.idUsuario,
                        nombresApellidos = item.nombresApellidos.ToString(),
                    });
                }
            }
            catch (Exception ex)
            {
                await mensaje.MostrarMensaje("Error", ex.Message);
            }
        }
        #endregion



        #region +------------- Cargando Propietarios Desde Web Service -----------------+
        private async void CargarPropietarioItem()
        {
            try
            {
                dynamic propietario = await Servicio.MetodoGet("ServicioPropietario.asmx", "MostrarPropietarios");
                propietarioItem.Clear();
                foreach (var item in propietario)
                {
                    propietarioItem.Add(new PropietarioItem
                    {
                        idPropietario = item.idPropietario,
                        nombre = item.nombre
                    });
                }
            }
            catch (Exception ex)
            {
                await mensaje.MostrarMensaje("Error", ex.Message);
            }
        }
        #endregion



        #region +-------------------- Guardar Nueva Obra -----------------------+
        private async void Guardar_Clicked(object sender, EventArgs e)
        {
            try
            {
                IsRunning = true;
                if (asignarPropietario.SelectedValue == null && asignarResponsable.SelectedValue == null)
                {
                    #region================ingresar solo obra=============================
                    if (string.IsNullOrEmpty(codigo.Text) || string.IsNullOrEmpty(nombre.Text))
                    {
                        await mensaje.MostrarMensaje("Agregar Obra", "Debe rellenar todos los campos.");
                        return;
                    }
                    object[,] variables = new object[,] { { "idObra", IdObra }, { "codigo", codigo.Text }, { "nombreObra", nombre.Text } };
                    dynamic result = await Servicio.MetodoGetString("ServicioObra.asmx", "IngresarObra", variables);
                    Mensaje = Convert.ToString(result);
                    if (result != null)
                    {
                        await mensaje.MostrarMensaje("Agregar Obra", Mensaje);

                        // Refrescando la lista
                        App.ListarObra.ObraItems.Clear();
                        App.ListarObra.CargarObraItems();
                        return;
                    }

                    #endregion
                }
                #region===========ingresar con responsable y propietario=========
                else
                {
                    int idPropietario = Convert.ToInt16(asignarPropietario.SelectedValue);
                    int idUsuario = Convert.ToInt16(asignarResponsable.SelectedValue);
                    object[,] variables = new object[,] { { "codigoObra", codigo.Text }, { "nombreObra", nombre.Text },
                   { "idPropietario",  idPropietario }, { "idUsuarioResponsable", idUsuario} };
                    dynamic result = await Servicio.MetodoGetString("ServicioPropietarioObra.asmx", "IngresarPropietarioResponsabledEnObra", variables);
                    Mensaje = Convert.ToString(result);
                    if (result != null)
                    {
                        await mensaje.MostrarMensaje("Agregar Obra con Responsable y Propietario", Mensaje);
                        return;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                await mensaje.MostrarMensaje("Agregar Obra", "Error en el dispositivo o URL incorrecto: " + ex.ToString());
            }
            finally
            {
                await Navigation.PopAsync();
                IsRunning = false;
            }
        } 
        #endregion



        #region Navegacion para el boton cancelar

        // boton cancelar
        private void Cancelar_Clicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        } 
        #endregion



        #region================== modificar obra =================================
        private async void modificarObra(object sender, EventArgs e)
        {
            try
            {
                IsRunning = true;
                if (string.IsNullOrEmpty(codigo.Text) || string.IsNullOrEmpty(nombre.Text))
                {
                await DisplayAlert("Modificar Obra", "Debe rellenar todos los campos.", "OK");
                return;
                 }
                object[,] variables = new object[,] { { "idObra", IdObra } , { "codigo", codigo.Text } , { "nombreObra", nombre.Text } };
                dynamic result = await Servicio.MetodoGetString("ServicioObra.asmx", "ModificarObra", variables);
                Mensaje = Convert.ToString(result);
                if (result != null)
                {
                    await mensaje.MostrarMensaje("Modificar Obra", Mensaje);
                    return;
                }
            }
            catch (Exception ex)
            {
                await mensaje.MostrarMensaje("Modificar Obra", "Error en el dispositivo o URL incorrecto: " + ex.ToString());
            }
            finally
            {
                isRunning = false;
            }
        }
        #endregion

    }
}

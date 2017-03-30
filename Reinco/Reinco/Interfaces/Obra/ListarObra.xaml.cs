﻿using Newtonsoft.Json;
using Reinco.Gestores;
using Reinco.Recursos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
    public partial class ListarObra : ContentPage, INotifyPropertyChanged
    {
        #region +---- Services ----+
        HttpClient Cliente = new HttpClient();
        WebService Servicio = new WebService(); 
        #endregion


        #region +---- Eventos ----+
        public event PropertyChangedEventHandler PropertyChanged; 
        #endregion


        #region +---- Atributos ----+
        public VentanaMensaje mensaje;
        string Mensaje;
        private bool isRefreshingObra { get; set; }
        #endregion


        #region +---- Propiedades ----+
        public ObservableCollection<ObraItem> ObraItems { get; set; }
        public bool IsRefreshingObra
        {
            set
            {
                if (isRefreshingObra != value)
                {
                    isRefreshingObra = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRefreshingObra"));
                }
            }
            get
            {
                return isRefreshingObra;
            }
        }
        #endregion


        #region +---- Comandos ----+
        public ICommand CrearObra { get; private set; }
        public ICommand RefreshObraCommand { get; private set; } 
        #endregion


        #region +---- Constructor ----+
        public ListarObra()
        {
            InitializeComponent();
            this.BindingContext = this; // Contexto de los Bindings Clase Actual Importante para que pueda funcionar el refresco de la lista con Gestos

            ObraItems = new ObservableCollection<ObraItem>();
            CargarObraItems();

            #region +---- Preparando Los Comandos ----+
            // Evento Crear Obra
            CrearObra = new Command(() =>
             {
                 Navigation.PushAsync(new AgregarObra());
             });

            // Evento Refrescar La Lista
            RefreshObraCommand = new Command( () =>
            {
                CargarObraItems();
                IsRefreshingObra = false;
            }); 
            #endregion
        } 
        #endregion


        #region +---- Definiendo Propiedad Global De esta Pagina ----+
        protected override void OnAppearing()
        {
            base.OnAppearing();
            App.ListarObra = this;
        }
        #endregion


        #region +---- Cargando las obras ----+
        public async void CargarObraItems()
        {
            try
            {
                dynamic result = await Servicio.MetodoGet("ServicioObra.asmx", "MostrarObras");
                foreach (var item in result)
                {
                    ObraItems.Add(new ObraItem
                    {
                        idObra = item.idObra,
                        nombre = item.nombre,
                        codigo = item.codigo,
                        colorObra = "#FF7777"
                    });
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Aceptar");
            }
        }
        #endregion
        

        #region +---- Evento Eliminar Obra ----+
        public async void eliminar(object sender, EventArgs e)
        {
            try { 
                var idObra = ((MenuItem)sender).CommandParameter;
                int IdObra = Convert.ToInt16(idObra);
                bool respuesta= await DisplayAlert("Eliminar", "Eliminar idObra = " + idObra, "Aceptar","Cancelar");
                object[,] variables = new object[,] { { "idObra", IdObra } };
                dynamic result = await Servicio.MetodoGetString("ServicioObra.asmx", "EliminarObra", variables);
                Mensaje = Convert.ToString(result);
                if (result!=null)
                {
                    await App.Current.MainPage.DisplayAlert("Eliminar Obra", Mensaje, "OK");
                    return;
                }
            }
            catch (Exception ex)
            {
                await mensaje.MostrarMensaje("Eliminar Obra", "Error en el dispositivo o URL incorrecto: " + ex.ToString());
            }
        }
        #endregion

    }
}

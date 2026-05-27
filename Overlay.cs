using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ImageOverlay
{
    // =========================================================================
    // FORMULARIO PRINCIPAL: PANEL DE CONTROL (TEMA OSCURO PREMIUM)
    // =========================================================================
    public class ControlForm : Form
    {
        // Importaciones de la API de Windows para Hotkeys Globales
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_F2_ID = 1;
        private const int HOTKEY_F3_ID = 2;
        private const int HOTKEY_F4_ID = 3;
        private const uint VK_F2 = 0x71; // Tecla F2
        private const uint VK_F3 = 0x72; // Tecla F3
        private const uint VK_F4 = 0x73; // Tecla F4

        // Controles de la Interfaz
        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblUrlHeader;
        private TextBox txtUrl;
        private Button btnLoadUrl;
        private Button btnLoadLocal;
        private Label lblStatusText;
        
        private Label lblAdjustTitle;
        private Label lblZoom;
        private TrackBar tbZoom;
        private Label lblOpacity;
        private TrackBar tbOpacity;
        
        private Label lblStateTitle;
        private Label lblLockStatus;
        private Button btnToggleLock;
        private Panel pnlShortcuts;
        private Label lblShortcutInfo1;
        private Label lblShortcutInfo2;
        private Label lblShortcutInfo3;

        // Referencia al Formulario del Overlay
        private OverlayForm _overlay;

        public ControlForm()
        {
            // Configurar propiedades del Formulario
            this.Text = "Panel de Control - Image Overlay";
            this.Size = new Size(410, 540);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(24, 24, 27); // Zinc 900
            this.ForeColor = Color.FromArgb(244, 244, 245); // Zinc 100
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.TopMost = true; // Mantener siempre arriba para control cómodo

            // Inicializar e inyectar controles de la interfaz
            ConstruirInterfaz();

            // Inicializar el Overlay Form
            _overlay = new OverlayForm(this);
            _overlay.Show();

            // Cargar imagen por defecto (si existe alguna)
            CargarImagenPorDefecto();
            
            // Actualizar la interfaz con los valores iniciales
            ActualizarInterfazDeEstado();
        }

        private void ConstruirInterfaz()
        {
            // --- 1. Cabecera (Header Panel) ---
            pnlHeader = new Panel();
            pnlHeader.Bounds = new Rectangle(0, 0, 410, 50);
            pnlHeader.BackColor = Color.FromArgb(39, 39, 42); // Zinc 800
            
            lblTitle = new Label();
            lblTitle.Text = "OVERLAY TRACING UTILITY";
            lblTitle.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(129, 140, 248); // Indigo 400
            lblTitle.Bounds = new Rectangle(15, 13, 300, 25);
            pnlHeader.Controls.Add(lblTitle);
            this.Controls.Add(pnlHeader);

            // --- 2. Sección de Origen (Carga Web y Local) ---
            lblUrlHeader = new Label();
            lblUrlHeader.Text = "Enlace Web de la Imagen (URL):";
            lblUrlHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblUrlHeader.ForeColor = Color.FromArgb(161, 161, 170); // Zinc 400
            lblUrlHeader.Bounds = new Rectangle(15, 65, 300, 18);
            this.Controls.Add(lblUrlHeader);

            txtUrl = new TextBox();
            txtUrl.Bounds = new Rectangle(15, 87, 260, 23);
            txtUrl.BackColor = Color.FromArgb(39, 39, 42); // Zinc 800
            txtUrl.ForeColor = Color.White;
            txtUrl.BorderStyle = BorderStyle.FixedSingle;
            txtUrl.Text = "https://";
            this.Controls.Add(txtUrl);

            btnLoadUrl = new Button();
            btnLoadUrl.Text = "Cargar Web";
            btnLoadUrl.Bounds = new Rectangle(285, 86, 100, 25);
            btnLoadUrl.FlatStyle = FlatStyle.Flat;
            btnLoadUrl.BackColor = Color.FromArgb(79, 70, 229); // Indigo 600
            btnLoadUrl.ForeColor = Color.White;
            btnLoadUrl.FlatAppearance.BorderSize = 0;
            btnLoadUrl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLoadUrl.Click += btnLoadUrl_Click;
            this.Controls.Add(btnLoadUrl);

            btnLoadLocal = new Button();
            btnLoadLocal.Text = "Seleccionar Archivo de Imagen Local";
            btnLoadLocal.Bounds = new Rectangle(15, 122, 370, 30);
            btnLoadLocal.FlatStyle = FlatStyle.Flat;
            btnLoadLocal.BackColor = Color.FromArgb(63, 63, 70); // Zinc 700
            btnLoadLocal.ForeColor = Color.White;
            btnLoadLocal.FlatAppearance.BorderSize = 0;
            btnLoadLocal.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLoadLocal.Click += btnLoadLocal_Click;
            this.Controls.Add(btnLoadLocal);

            lblStatusText = new Label();
            lblStatusText.Text = "Esperando imagen...";
            lblStatusText.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblStatusText.ForeColor = Color.FromArgb(113, 113, 122); // Zinc 500
            lblStatusText.Bounds = new Rectangle(15, 157, 370, 18);
            this.Controls.Add(lblStatusText);

            // --- 3. Sección de Ajustes (Sliders) ---
            lblAdjustTitle = new Label();
            lblAdjustTitle.Text = "Ajustes de Visualización";
            lblAdjustTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblAdjustTitle.ForeColor = Color.FromArgb(129, 140, 248); // Indigo 400
            lblAdjustTitle.Bounds = new Rectangle(15, 185, 300, 20);
            this.Controls.Add(lblAdjustTitle);

            lblZoom = new Label();
            lblZoom.Text = "Zoom: 100%";
            lblZoom.Bounds = new Rectangle(15, 210, 200, 18);
            lblZoom.ForeColor = Color.FromArgb(212, 212, 216); // Zinc 300
            this.Controls.Add(lblZoom);

            tbZoom = new TrackBar();
            tbZoom.Bounds = new Rectangle(10, 230, 380, 45);
            tbZoom.Minimum = 10;
            tbZoom.Maximum = 500;
            tbZoom.Value = 100;
            tbZoom.TickFrequency = 50;
            tbZoom.BackColor = Color.FromArgb(24, 24, 27);
            tbZoom.Scroll += tbZoom_Scroll;
            this.Controls.Add(tbZoom);

            lblOpacity = new Label();
            lblOpacity.Text = "Opacidad: 50%";
            lblOpacity.Bounds = new Rectangle(15, 275, 200, 18);
            lblOpacity.ForeColor = Color.FromArgb(212, 212, 216);
            this.Controls.Add(lblOpacity);

            tbOpacity = new TrackBar();
            tbOpacity.Bounds = new Rectangle(10, 295, 380, 45);
            tbOpacity.Minimum = 10;
            tbOpacity.Maximum = 100;
            tbOpacity.Value = 50;
            tbOpacity.TickFrequency = 10;
            tbOpacity.BackColor = Color.FromArgb(24, 24, 27);
            tbOpacity.Scroll += tbOpacity_Scroll;
            this.Controls.Add(tbOpacity);

            // --- 4. Estado de Interacción ---
            lblStateTitle = new Label();
            lblStateTitle.Text = "Estado de Interacción";
            lblStateTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblStateTitle.ForeColor = Color.FromArgb(129, 140, 248);
            lblStateTitle.Bounds = new Rectangle(15, 345, 300, 20);
            this.Controls.Add(lblStateTitle);

            lblLockStatus = new Label();
            lblLockStatus.Text = "BLOQUEADO (Click-Through)";
            lblLockStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblLockStatus.ForeColor = Color.FromArgb(239, 68, 68); // Red 500
            lblLockStatus.Bounds = new Rectangle(15, 370, 370, 20);
            this.Controls.Add(lblLockStatus);

            btnToggleLock = new Button();
            btnToggleLock.Text = "Desbloquear para Ajustar (F3)";
            btnToggleLock.Bounds = new Rectangle(15, 395, 370, 32);
            btnToggleLock.FlatStyle = FlatStyle.Flat;
            btnToggleLock.BackColor = Color.FromArgb(63, 63, 70); // Zinc 700
            btnToggleLock.ForeColor = Color.White;
            btnToggleLock.FlatAppearance.BorderSize = 0;
            btnToggleLock.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnToggleLock.Click += btnToggleLock_Click;
            this.Controls.Add(btnToggleLock);

            // --- 5. Atajos de Teclado (Footer) ---
            pnlShortcuts = new Panel();
            pnlShortcuts.Bounds = new Rectangle(0, 442, 410, 75);
            pnlShortcuts.BackColor = Color.FromArgb(39, 39, 42); // Zinc 800

            lblShortcutInfo1 = new Label();
            lblShortcutInfo1.Text = "[F2] Ocultar/Mostrar Overlay   |   [F3] Bloquear/Desbloquear   |   [F4] Salir";
            lblShortcutInfo1.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblShortcutInfo1.ForeColor = Color.FromArgb(199, 210, 254); // Indigo 200
            lblShortcutInfo1.Bounds = new Rectangle(12, 10, 390, 15);
            pnlShortcuts.Controls.Add(lblShortcutInfo1);

            lblShortcutInfo2 = new Label();
            lblShortcutInfo2.Text = "Modo Desbloqueado: Arrastra la imagen y usa la Rueda del Mouse para Zoom.";
            lblShortcutInfo2.Font = new Font("Segoe UI", 7.5F, FontStyle.Regular);
            lblShortcutInfo2.ForeColor = Color.FromArgb(161, 161, 170); // Zinc 400
            lblShortcutInfo2.Bounds = new Rectangle(12, 30, 390, 15);
            pnlShortcuts.Controls.Add(lblShortcutInfo2);

            lblShortcutInfo3 = new Label();
            lblShortcutInfo3.Text = "Doble clic en la imagen (desbloqueado) para restablecer zoom y posición.";
            lblShortcutInfo3.Font = new Font("Segoe UI", 7.5F, FontStyle.Regular);
            lblShortcutInfo3.ForeColor = Color.FromArgb(161, 161, 170);
            lblShortcutInfo3.Bounds = new Rectangle(12, 47, 390, 15);
            pnlShortcuts.Controls.Add(lblShortcutInfo3);

            this.Controls.Add(pnlShortcuts);
        }

        private void CargarImagenPorDefecto()
        {
            string[] nombresComunes = { "dibujo.png", "dibujo.jpg", "dibujo.jpeg" };
            string rutaBase = AppDomain.CurrentDomain.BaseDirectory;

            foreach (string nombre in nombresComunes)
            {
                string rutaCompleta = Path.Combine(rutaBase, nombre);
                if (File.Exists(rutaCompleta))
                {
                    try
                    {
                        Image img = Image.FromFile(rutaCompleta);
                        _overlay.EstablecerNuevaImagen(img);
                        lblStatusText.Text = string.Format("Imagen local '{0}' cargada al iniciar.", nombre);
                        lblStatusText.ForeColor = Color.FromArgb(34, 197, 94); // Green 500
                        return;
                    }
                    catch
                    {
                        // Continuar buscando si falla la carga
                    }
                }
            }
            lblStatusText.Text = "Sin imagen. Carga un archivo local o ingresa una URL web.";
        }

        // Acciones de Carga
        private void btnLoadLocal_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Archivos de Imagen|*.png;*.jpg;*.jpeg;*.bmp;*.gif";
                ofd.Title = "Seleccionar Imagen para Calcar";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Image img = Image.FromFile(ofd.FileName);
                        _overlay.EstablecerNuevaImagen(img);
                        lblStatusText.Text = string.Format("Imagen cargada: {0}", Path.GetFileName(ofd.FileName));
                        lblStatusText.ForeColor = Color.FromArgb(34, 197, 94);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al cargar la imagen:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnLoadUrl_Click(object sender, EventArgs e)
        {
            string url = txtUrl.Text.Trim();
            if (string.IsNullOrEmpty(url) || url == "https://")
            {
                MessageBox.Show("Por favor, ingresa una URL válida.", "URL Vacía", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lblStatusText.Text = "Descargando imagen desde la web...";
            lblStatusText.ForeColor = Color.FromArgb(245, 158, 11); // Amber 500

            using (WebClient client = new WebClient())
            {
                // Configurar User-Agent simulando navegador para prevenir bloqueos HTTP 403
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)");
                
                client.DownloadDataCompleted += (s, ev) =>
                {
                    if (ev.Error != null)
                    {
                        MessageBox.Show("Error al descargar la imagen:\n" + ev.Error.Message, "Error de Red", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        lblStatusText.Text = "Error en la descarga web.";
                        lblStatusText.ForeColor = Color.FromArgb(239, 68, 68);
                    }
                    else
                    {
                        try
                        {
                            using (MemoryStream ms = new MemoryStream(ev.Result))
                            {
                                Image img = Image.FromStream(ms);
                                _overlay.EstablecerNuevaImagen(img);
                                lblStatusText.Text = "Imagen web descargada y cargada con éxito.";
                                lblStatusText.ForeColor = Color.FromArgb(34, 197, 94);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("El recurso descargado no es una imagen válida:\n" + ex.Message, "Formato Inválido", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            lblStatusText.Text = "Error de formato de imagen.";
                            lblStatusText.ForeColor = Color.FromArgb(239, 68, 68);
                        }
                    }
                };

                try
                {
                    client.DownloadDataAsync(new Uri(url));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("La URL provista no tiene un formato válido:\n" + ex.Message, "Formato de URL Incorrecto", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatusText.Text = "URL con formato inválido.";
                    lblStatusText.ForeColor = Color.FromArgb(239, 68, 68);
                }
            }
        }

        // Controladores de Sliders
        private void tbZoom_Scroll(object sender, EventArgs e)
        {
            double scale = tbZoom.Value / 100.0;
            lblZoom.Text = string.Format("Zoom: {0}%", tbZoom.Value);
            _overlay.AjustarEscalaZoom(scale);
        }

        private void tbOpacity_Scroll(object sender, EventArgs e)
        {
            double opacity = tbOpacity.Value / 100.0;
            lblOpacity.Text = string.Format("Opacidad: {0}%", tbOpacity.Value);
            _overlay.AjustarOpacidad(opacity);
        }

        private void btnToggleLock_Click(object sender, EventArgs e)
        {
            ToggleInteractivity();
        }

        // Métodos de Interfaz públicos para sincronización desde el Overlay
        public void ActualizarSliderZoom(double zoom)
        {
            int val = (int)(zoom * 100);
            if (val < tbZoom.Minimum) val = tbZoom.Minimum;
            if (val > tbZoom.Maximum) val = tbZoom.Maximum;
            tbZoom.Value = val;
            lblZoom.Text = string.Format("Zoom: {0}%", val);
        }

        public void ToggleInteractivity()
        {
            bool actualBloqueado = _overlay.AlternarBloqueoInteraccion();
            ActualizarInterfazDeEstado();
        }

        public void AlternarVisibilidadOverlay()
        {
            _overlay.AlternarVisibilidad();
        }

        private void ActualizarInterfazDeEstado()
        {
            if (_overlay == null) return;
            
            bool bloqueado = _overlay.EstaBloqueado;
            if (bloqueado)
            {
                lblLockStatus.Text = "BLOQUEADO (Transparente a clics)";
                lblLockStatus.ForeColor = Color.FromArgb(239, 68, 68); // Red 500
                btnToggleLock.Text = "DESBLOQUEAR IMAGEN PARA AJUSTAR (F3)";
                btnToggleLock.BackColor = Color.FromArgb(79, 70, 229); // Indigo 600
            }
            else
            {
                lblLockStatus.Text = "DESBLOQUEADO (Interactivo para arrastrar y zoom)";
                lblLockStatus.ForeColor = Color.FromArgb(34, 197, 94); // Green 500
                btnToggleLock.Text = "BLOQUEAR IMAGEN PARA EMPEZAR A CALCAR (F3)";
                btnToggleLock.BackColor = Color.FromArgb(220, 38, 38); // Red 600
            }
        }

        // Gestión del ciclo de vida de los Hotkeys Globales
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RegisterHotKey(this.Handle, HOTKEY_F2_ID, 0, VK_F2); // F2
            RegisterHotKey(this.Handle, HOTKEY_F3_ID, 0, VK_F3); // F3
            RegisterHotKey(this.Handle, HOTKEY_F4_ID, 0, VK_F4); // F4
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            UnregisterHotKey(this.Handle, HOTKEY_F2_ID);
            UnregisterHotKey(this.Handle, HOTKEY_F3_ID);
            UnregisterHotKey(this.Handle, HOTKEY_F4_ID);
            base.OnHandleDestroyed(e);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY)
            {
                int hotkeyId = m.WParam.ToInt32();
                if (hotkeyId == HOTKEY_F2_ID)
                {
                    AlternarVisibilidadOverlay();
                }
                else if (hotkeyId == HOTKEY_F3_ID)
                {
                    ToggleInteractivity();
                }
                else if (hotkeyId == HOTKEY_F4_ID)
                {
                    Application.Exit();
                }
            }
            base.WndProc(ref m);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Application.Exit();
            base.OnFormClosed(e);
        }
    }

    // =========================================================================
    // FORMULARIO OVERLAY: PANTALLA COMPLETA TRANSPARENTE E INTERACTIVA
    // =========================================================================
    public class OverlayForm : Form
    {
        // Importaciones Win32 para Click-Through dinámico
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_TOPMOST = 0x00000008;

        // Variables de Estado Interno del Overlay
        private ControlForm _controlPanel;
        private Image _imagenOriginal;
        private double _zoomScale = 1.0;
        private Point _offset = new Point(0, 0);
        
        private bool _estaVisible = true;
        private bool _estaBloqueado = true; // Por defecto inicia click-through
        private double _opacidadActual = 0.50; // Guardar la opacidad real definida por el usuario

        // Variables de Arrastre con Mouse
        private bool _estaArrastrando = false;
        private Point _mouseStartPos;
        private Point _offsetStartPos;

        public bool EstaBloqueado
        {
            get { return _estaBloqueado; }
        }

        public OverlayForm(ControlForm panel)
        {
            _controlPanel = panel;

            // Configurar propiedades de la ventana de overlay
            this.Text = "Overlay Canvas";
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.DoubleBuffered = true; // Prevenir parpadeo al rediseñar

            // Configurar color de transparencia total
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;

            // Configurar opacidad inicial
            this.Opacity = _opacidadActual;

            // Configurar a pantalla completa abarcando todo el escritorio virtual (soporte multi-monitor)
            this.StartPosition = FormStartPosition.Manual;
            this.Bounds = SystemInformation.VirtualScreen;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // Iniciamos con click-through activo, modo ToolWindow (oculto en Alt+Tab) y siempre arriba
                cp.ExStyle |= WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_TOPMOST;
                return cp;
            }
        }

        public void EstablecerNuevaImagen(Image nuevaImg)
        {
            if (_imagenOriginal != null)
            {
                _imagenOriginal.Dispose();
            }

            _imagenOriginal = nuevaImg;
            
            // Centrar la imagen en la pantalla al cargar por primera vez
            if (_imagenOriginal != null)
            {
                _zoomScale = 1.0;
                _offset.X = (this.Width - _imagenOriginal.Width) / 2;
                _offset.Y = (this.Height - _imagenOriginal.Height) / 2;
                
                _controlPanel.ActualizarSliderZoom(_zoomScale);
            }
            
            this.Invalidate();
        }

        public void AjustarEscalaZoom(double zoom)
        {
            _zoomScale = zoom;
            this.Invalidate();
        }

        public void AjustarOpacidad(double opacidad)
        {
            _opacidadActual = opacidad;
            // Solo aplicar directamente si el overlay se encuentra visible
            if (_estaVisible)
            {
                this.Opacity = _opacidadActual;
            }
        }

        public bool AlternarBloqueoInteraccion()
        {
            _estaBloqueado = !_estaBloqueado;
            AplicarEstiloClickThrough(_estaBloqueado);
            return _estaBloqueado;
        }

        private void AplicarEstiloClickThrough(bool bloquear)
        {
            IntPtr hwnd = this.Handle;
            int style = GetWindowLong(hwnd, GWL_EXSTYLE);
            
            if (bloquear)
            {
                SetWindowLong(hwnd, GWL_EXSTYLE, style | WS_EX_TRANSPARENT);
            }
            else
            {
                SetWindowLong(hwnd, GWL_EXSTYLE, style & ~WS_EX_TRANSPARENT);
            }
            this.Invalidate();
        }

        public void AlternarVisibilidad()
        {
            _estaVisible = !_estaVisible;
            if (_estaVisible)
            {
                this.Opacity = _opacidadActual; // Corregido: Usa la opacidad real del overlay en lugar de la del control panel
            }
            else
            {
                this.Opacity = 0.0;
            }
        }

        // Pintar la imagen con escala y offsets usando GDI+ de alta calidad
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            if (_imagenOriginal != null && _estaVisible)
            {
                Graphics g = e.Graphics;
                
                // Configurar renderizado premium
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                int w = (int)(_imagenOriginal.Width * _zoomScale);
                int h = (int)(_imagenOriginal.Height * _zoomScale);

                g.DrawImage(_imagenOriginal, _offset.X, _offset.Y, w, h);

                // Si está desbloqueado, dibujar un borde sutil alrededor de la imagen para facilitar su ubicación
                if (!_estaBloqueado)
                {
                    using (Pen pen = new Pen(Color.FromArgb(129, 140, 248), 2)) // Borde índigo de 2px
                    {
                        g.DrawRectangle(pen, _offset.X, _offset.Y, w, h);
                    }
                }
            }
        }

        // ==========================================
        // EVENTOS DEL MOUSE EN MODO DESBLOQUEADO
        // ==========================================
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!_estaBloqueado && _imagenOriginal != null && e.Button == MouseButtons.Left)
            {
                // Comprobar si el clic se realizó dentro de los límites de la imagen pintada
                int w = (int)(_imagenOriginal.Width * _zoomScale);
                int h = (int)(_imagenOriginal.Height * _zoomScale);
                Rectangle imgBounds = new Rectangle(_offset.X, _offset.Y, w, h);

                if (imgBounds.Contains(e.Location))
                {
                    _estaArrastrando = true;
                    _mouseStartPos = e.Location;
                    _offsetStartPos = _offset;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_estaArrastrando)
            {
                int dx = e.X - _mouseStartPos.X;
                int dy = e.Y - _mouseStartPos.Y;
                _offset = new Point(_offsetStartPos.X + dx, _offsetStartPos.Y + dy);
                this.Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _estaArrastrando = false;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!_estaBloqueado && _imagenOriginal != null)
            {
                // Zoom interactivo con rueda de scroll
                double factorZoom = e.Delta > 0 ? 1.05 : 0.95;
                double nuevoZoom = _zoomScale * factorZoom;

                // Limitar entre 10% (0.1) y 500% (5.0)
                if (nuevoZoom >= 0.1 && nuevoZoom <= 5.0)
                {
                    // Zoom focalizado en el cursor del mouse (Mantiene la zona del mouse fija)
                    int mouseX = e.X;
                    int mouseY = e.Y;

                    // Fracción relativa del mouse sobre la imagen actual
                    double relX = (mouseX - _offset.X) / _zoomScale;
                    double relY = (mouseY - _offset.Y) / _zoomScale;

                    _zoomScale = nuevoZoom;

                    // Ajustar compensación de desplazamiento
                    _offset.X = (int)(mouseX - relX * _zoomScale);
                    _offset.Y = (int)(mouseY - relY * _zoomScale);

                    this.Invalidate();

                    // Sincronizar el valor con el control de zoom en el Control Panel
                    _controlPanel.ActualizarSliderZoom(_zoomScale);
                }
            }
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            // Doble clic para restablecer al tamaño por defecto y centrar
            if (!_estaBloqueado && _imagenOriginal != null)
            {
                RestablecerEstadoPorDefecto();
            }
        }

        private void RestablecerEstadoPorDefecto()
        {
            _zoomScale = 1.0;
            _offset.X = (this.Width - _imagenOriginal.Width) / 2;
            _offset.Y = (this.Height - _imagenOriginal.Height) / 2;
            
            _controlPanel.ActualizarSliderZoom(_zoomScale);
            this.Invalidate();
        }
    }

    // =========================================================================
    // PUNTO DE ENTRADA PRINCIPAL (STA THREAD)
    // =========================================================================
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Habilitar protocolos TLS 1.2, TLS 1.1 y TLS 1.0 para compatibilidad con servidores web modernos
            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)768 | (SecurityProtocolType)192;
            }
            catch
            {
                // Ignorar en sistemas donde no esté soportado
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Iniciar arrancando directamente con la interfaz del Panel de Control
            Application.Run(new ControlForm());
        }
    }
}

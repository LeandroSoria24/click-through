#  Image Overlay - Tracing Utility

Una herramienta premium y ligera desarrollada en **C# y Windows Forms** diseñada para artistas, diseñadores, modeladores 3D y desarrolladores. Permite superponer cualquier imagen (local o de internet) sobre la pantalla con opacidad ajustable, permitiendo dibujar, calcar o comparar diseños directamente sobre otras aplicaciones abiertas.

---

##  Cómo Funciona la Aplicación

La aplicación se compone de **dos ventanas (Forms) principales** que trabajan de forma sincronizada para ofrecer una experiencia fluida:

### 1. Panel de Control (`ControlForm`)
Es la ventana principal de configuración. Cuenta con un diseño oscuro premium moderno (basado en la paleta de colores *Tailwind Zinc/Indigo*) que permanece siempre visible sobre las demás ventanas (`TopMost = true`). Desde aquí puedes:
*   **Cargar Imágenes**: Admite arrastrar o seleccionar archivos locales (`PNG`, `JPG`, `JPEG`, `BMP`, `GIF`) y descargar imágenes directamente mediante una URL web (`https://`).
*   **Ajustar la Opacidad**: Barra deslizante (TrackBar) desde `10%` hasta `100%` para regular el nivel de transparencia de la imagen superpuesta.
*   **Ajustar el Zoom**: Control de escala de la imagen desde `10%` hasta `500%`.
*   **Monitorear el Estado**: Indicadores visuales interactivos que muestran si el overlay está en modo interactivo o de calco.

### 2. Lienzo Superpuesto (`OverlayForm`)
Es una ventana invisible a pantalla completa (`FormBorderStyle = None` y tamaño adaptado a `VirtualScreen` para soportar configuraciones de **múltiples monitores**). Utiliza una clave de transparencia cromática (`TransparencyKey = Color.Magenta`) para que todo el fondo sea invisible y transparente al mouse, dibujando únicamente la imagen elegida por el usuario con renderizado bicúbico de alta calidad.

###  Modos de Interacción

La aplicación cuenta con dos estados de interacción cruciales:

| Modo / Estado | Descripción | Comportamiento del Mouse |
| :--- | :--- | :--- |
| **Bloqueado (Click-Through)** | **Modo de Calco Activo**. La imagen es visible pero "fantasma". | Los clics del mouse pasan de largo a través de la imagen, permitiéndote interactuar, pintar o modelar en la app que está al fondo (ej. Photoshop, Blender, Illustrator). |
| **Desbloqueado (Interactivo)** | **Modo de Configuración**. Permite ajustar y posicionar el lienzo. | Puedes arrastrar la imagen manteniendo pulsado el clic izquierdo, hacer **Zoom focalizado** con la rueda del ratón (el zoom se centra donde apuntas con el cursor) y hacer **Doble clic** para restablecer la imagen a su estado original y centrado. |

###  Atajos de Teclado Globales (Hotkeys)
Registrados a nivel del sistema operativo. Funcionan incluso cuando la aplicación está minimizada o no tiene el foco activo:

*   **`[F2]`**: Ocultar / Mostrar el Overlay (apaga temporalmente la imagen sin perder tu progreso).
*   **`[F3]`**: Alternar entre **Modo Bloqueado** (Calco) y **Modo Desbloqueado** (Ajustes).
*   **`[F4]`**: Cerrar la aplicación de forma segura e inmediata.

---

##  Seguridad y Robustez

El software ha sido diseñado bajo estrictos estándares de seguridad y eficiencia:

### 1. Seguridad en la Integración con Windows (Win32 API)
La aplicación interactúa con el sistema operativo de forma transparente y limpia:
*   **P/Invoke Seguro**: Se invocan funciones oficiales y estándar de la API de Windows (`RegisterHotKey`, `UnregisterHotKey`, `GetWindowLong`, `SetWindowLong`) sin inyectar DLLs externas ni utilizar ganchos globales de teclado (*Keyboard Hooks*) de bajo nivel, lo que **evita falsos positivos** en programas antivirus.
*   **Liberación de Recursos**: Cuando se cierra el programa, el método `OnHandleDestroyed` ejecuta un ciclo de limpieza que desregistra inmediatamente los hotkeys del sistema, garantizando que el sistema operativo recupere las teclas `F2`, `F3` y `F4` sin conflictos.

### 2. Seguridad en Redes y Descargas
*   **Cifrado Moderno**: Para la carga de imágenes por URL, se configura explícitamente el soporte para los protocolos seguros **TLS 1.2, TLS 1.1 y TLS 1.0** a través de `ServicePointManager.SecurityProtocol`, garantizando conexiones seguras mediante HTTPS.
*   **Resiliencia HTTP**: Se inyecta un encabezado `User-Agent` que simula un navegador moderno para prevenir bloqueos por políticas de seguridad (error `HTTP 403 Forbidden`) en servidores web comunes.
*   **Control de Hilos Asíncronos**: La descarga se realiza de manera no bloqueante (`DownloadDataAsync`) en un hilo secundario. Así, si la conexión es lenta o el servidor no responde, el Panel de Control no se congela y el usuario puede cancelar o reintentar la acción de forma segura.

### 3. Seguridad de Memoria
*   **Entorno Administrado**: Al estar compilado sobre el **.NET Runtime**, el código se ejecuta en un ambiente seguro que evita desbordamientos de búfer (*buffer overflows*), punteros huérfanos o corrupción de memoria.
*   **Manejo de Excepciones**: Los flujos de carga de archivos locales y descargas web se ejecutan dentro de bloques estructurados de captura de errores (`try-catch`), asegurando que las imágenes corruptas o archivos no válidos se descarten con un mensaje de alerta en lugar de provocar caídas inesperadas del programa.
*   **Liberación de Recursos Gráficos**: Implementa el método `.Dispose()` sobre objetos GDI+ (`Image`, `Pen`, `Graphics`) para evitar fugas de memoria RAM e integrarse adecuadamente con el recolector de basura (*Garbage Collector*).

---

##  Cómo Fue Creado

La aplicación se construyó siguiendo una metodología ágil y modular:

1.  **Lenguaje y Framework**: Escrito en **C#** utilizando **Windows Forms** sobre el **.NET Framework**. Esto proporciona una integración perfecta con el sistema operativo Windows y llamadas directas de bajo nivel sumamente veloces.
2.  **Diseño de Interfaz Premium**: Se rompió el esquema gris clásico de Windows Forms aplicando un diseño oscuro e inmersivo. La UI cuenta con bordes limpios, fuentes tipográficas modernas (`Segoe UI` seminegrita) y un sistema de colores curado que destaca los estados activos con tonos verdes e índigo, y los bloqueos con tonos rojos.
3.  **Lógica del Algoritmo de Zoom y Arrastre**:
    *   **Zoom Centrado en el Cursor**: En lugar de escalar la imagen desde la esquina superior izquierda (que desubica el elemento al hacer zoom), la aplicación calcula la posición del cursor respecto a los límites de la imagen original. Al aplicar la rueda del ratón, reajusta las coordenadas `X` e `Y` (`_offset`) de la imagen para que el punto específico bajo el mouse permanezca completamente inmóvil, dando una sensación de precisión quirúrgica.
    *   **Renderizado de Alta Calidad**: GDI+ se configura en modo de interpolación bicúbica de alta calidad (`HighQualityBicubic`) y antialiasing, evitando que la imagen se pixele notablemente cuando el usuario le aplica un zoom de hasta `500%`.
4.  **Estilo de Ventana Extendido (Click-Through)**: El núcleo técnico detrás del calco sin fricciones reside en la manipulación dinámica del estilo extendido de la ventana (`GWL_EXSTYLE`). Al alternar la tecla `F3`, la aplicación añade o remueve el bit `WS_EX_TRANSPARENT`. Al activarlo, el kernel de Windows ignora por completo la ventana de dibujo en la fase de prueba de clics (*Hit-Testing*), enviando la interacción del mouse directamente a la capa inferior.

---

##  Requisitos de Compilación y Ejecución

*   **Sistema Operativo**: Windows 7 / 8 / 10 / 11.
*   **Framework**: .NET Framework 4.5 o superior.
*   **Herramienta de compilación**: Visual Studio, MSBuild o compilación directa por consola de C# (`csc.exe`).

### Instrucciones para Compilar por Consola:
Abre una terminal con acceso al compilador de C# (Developer PowerShell de Visual Studio) y ejecuta:
```bash
csc /target:winexe /out:Overlay.exe Overlay.cs
```
Esto generará un ejecutable standalone `Overlay.exe` optimizado de solo unos pocos kilobytes, listo para ser utilizado en cualquier computador sin instalador.

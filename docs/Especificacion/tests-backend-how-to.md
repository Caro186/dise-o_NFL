# Guía de Pruebas Unitarias del Backend - NFL Fantasy API

Esta guía explica cómo ejecutar las pruebas unitarias del backend del proyecto "New Generation NFL Fantasy".

## 📋 Requisitos Previos

Antes de ejecutar las pruebas, asegúrate de tener instalado:

- **.NET 9 SDK** o superior
  - Puedes verificar tu versión ejecutando: `dotnet --version`
  - Si no lo tienes, descárgalo desde [dotnet.microsoft.com](https://dotnet.microsoft.com/download)

- **Acceso a la carpeta `backend/`** del proyecto

## 🚀 Cómo Ejecutar las Pruebas

Abre una terminal y navega a la carpeta `backend/`:

```bash
cd backend
```

### Ejecutar Todas las Pruebas

Para ejecutar todas las pruebas unitarias:

```bash
dotnet test
```

O desde la raíz del proyecto:

```bash
dotnet test NFLFantasyAPI.sln
```

### Ejecutar Pruebas por Clase

**AuthServiceTests** (15 pruebas):
```bash
dotnet test --filter "FullyQualifiedName~AuthServiceTests"
```

**LigaServiceTests** (12 pruebas):
```bash
dotnet test --filter "FullyQualifiedName~LigaServiceTests"
```

**JugadorServiceTests** (18 pruebas):
```bash
dotnet test --filter "FullyQualifiedName~JugadorServiceTests"
```

**AuthControllerTests** (13 pruebas):
```bash
dotnet test --filter "FullyQualifiedName~AuthControllerTests"
```

### Ejecutar Pruebas Individuales

#### AuthServiceTests

Verifica que el registro retorne error cuando el email ya existe.
```bash
dotnet test --filter "FullyQualifiedName~RegisterAsync_WhenEmailAlreadyExists_ReturnsBadRequest"
```

Verifica que el registro retorne éxito con datos válidos.
```bash
dotnet test --filter "FullyQualifiedName~RegisterAsync_WhenValidData_ReturnsOkWithUsuario"
```

Verifica que el registro maneje excepciones correctamente.
```bash
dotnet test --filter "FullyQualifiedName~RegisterAsync_WhenExceptionThrown_ReturnsError"
```

Verifica que el login retorne error cuando el usuario no existe.
```bash
dotnet test --filter "FullyQualifiedName~LoginAsync_WhenUserNotFound_ReturnsBadRequest"
```

Verifica que el login retorne error cuando la cuenta está bloqueada.
```bash
dotnet test --filter "FullyQualifiedName~LoginAsync_WhenAccountBlocked_ReturnsBadRequest"
```

Verifica que el login retorne error con contraseña inválida.
```bash
dotnet test --filter "FullyQualifiedName~LoginAsync_WhenInvalidPassword_ReturnsBadRequest"
```

Verifica que el login bloquee la cuenta tras 5 intentos fallidos.
```bash
dotnet test --filter "FullyQualifiedName~LoginAsync_WhenInvalidPassword5Times_BlocksAccount"
```

Verifica que el login retorne éxito con credenciales válidas.
```bash
dotnet test --filter "FullyQualifiedName~LoginAsync_WhenValidCredentials_ReturnsOkWithToken"
```

Verifica que el login maneje excepciones correctamente.
```bash
dotnet test --filter "FullyQualifiedName~LoginAsync_WhenExceptionThrown_ReturnsError"
```

Verifica que el desbloqueo retorne error cuando el usuario no existe.
```bash
dotnet test --filter "FullyQualifiedName~DesbloquearCuentaAsync_WhenUserNotFound_ReturnsBadRequest"
```

Verifica que el desbloqueo active la cuenta correctamente.
```bash
dotnet test --filter "FullyQualifiedName~DesbloquearCuentaAsync_WhenValidUser_UnblocksAccount"
```

Verifica que obtener usuario retorne error cuando no existe.
```bash
dotnet test --filter "FullyQualifiedName~GetUsuarioAsync_WhenUserNotFound_ReturnsBadRequest"
```

Verifica que obtener usuario retorne éxito cuando existe.
```bash
dotnet test --filter "FullyQualifiedName~GetUsuarioAsync_WhenUserExists_ReturnsOk"
```

Verifica que eliminar usuario retorne error cuando no existe.
```bash
dotnet test --filter "FullyQualifiedName~DeleteUsuarioAsync_WhenUserNotFound_ReturnsBadRequest"
```

Verifica que eliminar usuario funcione correctamente cuando existe.
```bash
dotnet test --filter "FullyQualifiedName~DeleteUsuarioAsync_WhenUserExists_DeletesUser"
```

#### LigaServiceTests

Verifica que obtener todas las ligas retorne éxito cuando existen ligas.
```bash
dotnet test --filter "FullyQualifiedName~GetAllAsync_WhenLigasExist_ReturnsOkWithLigas"
```

Verifica que obtener todas las ligas retorne éxito con lista vacía.
```bash
dotnet test --filter "FullyQualifiedName~GetAllAsync_WhenNoLigas_ReturnsOkWithEmptyList"
```

Verifica que obtener liga por ID retorne error cuando no existe.
```bash
dotnet test --filter "FullyQualifiedName~GetByIdAsync_WhenLigaNotFound_ReturnsBadRequest"
```

Verifica que obtener liga por ID retorne éxito cuando existe.
```bash
dotnet test --filter "FullyQualifiedName~GetByIdAsync_WhenLigaExists_ReturnsOk"
```

Verifica que crear liga funcione correctamente con datos válidos.
```bash
dotnet test --filter "FullyQualifiedName~CreateAsync_WhenValidData_CreatesLiga"
```

Verifica que actualizar liga retorne error cuando no existe.
```bash
dotnet test --filter "FullyQualifiedName~UpdateAsync_WhenLigaNotFound_ReturnsBadRequest"
```

Verifica que actualizar liga funcione correctamente cuando existe.
```bash
dotnet test --filter "FullyQualifiedName~UpdateAsync_WhenLigaExists_UpdatesLiga"
```

Verifica que eliminar liga retorne error cuando no existe.
```bash
dotnet test --filter "FullyQualifiedName~DeleteAsync_WhenLigaNotFound_ReturnsBadRequest"
```

Verifica que eliminar liga funcione correctamente cuando existe.
```bash
dotnet test --filter "FullyQualifiedName~DeleteAsync_WhenLigaExists_DeletesLiga"
```

Verifica que unirse a liga retorne error cuando la liga no existe.
```bash
dotnet test --filter "FullyQualifiedName~UnirseLigaAsync_WhenLigaNotFound_ReturnsBadRequest"
```

Verifica que unirse a liga retorne error con contraseña inválida.
```bash
dotnet test --filter "FullyQualifiedName~UnirseLigaAsync_WhenInvalidPassword_ReturnsBadRequest"
```

Verifica que unirse a liga retorne error cuando la liga está llena.
```bash
dotnet test --filter "FullyQualifiedName~UnirseLigaAsync_WhenLigaFull_ReturnsBadRequest"
```

Verifica que unirse a liga retorne error cuando el usuario no existe.
```bash
dotnet test --filter "FullyQualifiedName~UnirseLigaAsync_WhenUsuarioNotFound_ReturnsBadRequest"
```

Verifica que unirse a liga retorne error cuando el equipo no existe.
```bash
dotnet test --filter "FullyQualifiedName~UnirseLigaAsync_WhenEquipoNotFound_ReturnsBadRequest"
```

Verifica que unirse a liga retorne error cuando el equipo ya está en otra liga.
```bash
dotnet test --filter "FullyQualifiedName~UnirseLigaAsync_WhenEquipoAlreadyInLiga_ReturnsBadRequest"
```

Verifica que unirse a liga funcione correctamente con datos válidos.
```bash
dotnet test --filter "FullyQualifiedName~UnirseLigaAsync_WhenValidData_JoinsLiga"
```

#### JugadorServiceTests

Verifica que obtener todos los jugadores retorne éxito cuando existen jugadores.
```bash
dotnet test --filter "FullyQualifiedName~GetAllAsync_WhenJugadoresExist_ReturnsOkWithJugadores"
```

Verifica que obtener todos los jugadores retorne éxito con lista vacía.
```bash
dotnet test --filter "FullyQualifiedName~GetAllAsync_WhenNoJugadores_ReturnsOkWithEmptyList"
```

Verifica que obtener jugador por ID retorne error cuando no existe.
```bash
dotnet test --filter "FullyQualifiedName~GetByIdAsync_WhenJugadorNotFound_ReturnsBadRequest"
```

Verifica que obtener jugador por ID retorne éxito cuando existe.
```bash
dotnet test --filter "FullyQualifiedName~GetByIdAsync_WhenJugadorExists_ReturnsOk"
```

Verifica que crear jugador retorne error cuando el equipo NFL no existe.
```bash
dotnet test --filter "FullyQualifiedName~CreateAsync_WhenEquipoNFLNotFound_ReturnsBadRequest"
```

Verifica que crear jugador retorne error cuando el jugador está duplicado.
```bash
dotnet test --filter "FullyQualifiedName~CreateAsync_WhenJugadorDuplicado_ReturnsBadRequest"
```

Verifica que crear jugador funcione correctamente con datos válidos.
```bash
dotnet test --filter "FullyQualifiedName~CreateAsync_WhenValidData_CreatesJugador"
```

Verifica que crear jugador retorne error con posición inválida.
```bash
dotnet test --filter "FullyQualifiedName~CreateAsync_WhenInvalidPosition_ReturnsBadRequest"
```

Verifica que actualizar jugador retorne error cuando no existe.
```bash
dotnet test --filter "FullyQualifiedName~UpdateAsync_WhenJugadorNotFound_ReturnsBadRequest"
```

Verifica que actualizar jugador funcione correctamente cuando existe.
```bash
dotnet test --filter "FullyQualifiedName~UpdateAsync_WhenJugadorExists_UpdatesJugador"
```

Verifica que eliminar jugador retorne error cuando no existe.
```bash
dotnet test --filter "FullyQualifiedName~DeleteAsync_WhenJugadorNotFound_ReturnsBadRequest"
```

Verifica que eliminar jugador permanentemente funcione correctamente.
```bash
dotnet test --filter "FullyQualifiedName~DeleteAsync_WhenPermanent_DeletesJugador"
```

Verifica que eliminar jugador (no permanente) desactive el jugador.
```bash
dotnet test --filter "FullyQualifiedName~DeleteAsync_WhenNotPermanent_DeactivatesJugador"
```

Verifica que obtener jugadores por equipo retorne error cuando el equipo no existe.
```bash
dotnet test --filter "FullyQualifiedName~GetByEquipoAsync_WhenEquipoNotFound_ReturnsBadRequest"
```

Verifica que obtener jugadores por equipo retorne éxito cuando el equipo existe.
```bash
dotnet test --filter "FullyQualifiedName~GetByEquipoAsync_WhenEquipoExists_ReturnsOk"
```

Verifica que obtener jugadores por posición retorne éxito cuando existen jugadores.
```bash
dotnet test --filter "FullyQualifiedName~GetByPosicionAsync_WhenJugadoresExist_ReturnsOk"
```

#### AuthControllerTests

Verifica que el endpoint de registro retorne éxito cuando el servicio retorna éxito.
```bash
dotnet test --filter "FullyQualifiedName~Register_WhenServiceReturnsOk_ReturnsOk"
```

Verifica que el endpoint de registro retorne error cuando el servicio retorna error.
```bash
dotnet test --filter "FullyQualifiedName~Register_WhenServiceReturnsBadRequest_ReturnsBadRequest"
```

Verifica que el endpoint de login retorne éxito cuando el servicio retorna éxito.
```bash
dotnet test --filter "FullyQualifiedName~Login_WhenServiceReturnsOk_ReturnsOk"
```

Verifica que el endpoint de login retorne error cuando el servicio retorna error.
```bash
dotnet test --filter "FullyQualifiedName~Login_WhenServiceReturnsBadRequest_ReturnsBadRequest"
```

Verifica que el endpoint de desbloquear retorne éxito cuando el servicio retorna éxito.
```bash
dotnet test --filter "FullyQualifiedName~Desbloquear_WhenServiceReturnsOk_ReturnsOk"
```

Verifica que el endpoint de obtener usuarios retorne éxito cuando el servicio retorna éxito.
```bash
dotnet test --filter "FullyQualifiedName~GetUsuarios_WhenServiceReturnsOk_ReturnsOk"
```

Verifica que el endpoint de obtener usuario retorne éxito cuando el servicio retorna éxito.
```bash
dotnet test --filter "FullyQualifiedName~GetUsuario_WhenServiceReturnsOk_ReturnsOk"
```

Verifica que el endpoint de obtener usuario retorne error cuando el servicio retorna error.
```bash
dotnet test --filter "FullyQualifiedName~GetUsuario_WhenServiceReturnsBadRequest_ReturnsBadRequest"
```

Verifica que el endpoint de eliminar usuario retorne éxito cuando el servicio retorna éxito.
```bash
dotnet test --filter "FullyQualifiedName~DeleteUsuario_WhenServiceReturnsOk_ReturnsOk"
```

Verifica que el endpoint de eliminar usuario retorne error cuando el servicio retorna error.
```bash
dotnet test --filter "FullyQualifiedName~DeleteUsuario_WhenServiceReturnsBadRequest_ReturnsBadRequest"
```

### Ver Detalles de Ejecución

Para ver información más detallada:

```bash
dotnet test --verbosity normal
```

## ✅ Resultados Esperados

### Cuando las Pruebas Pasan Correctamente

Deberías ver un mensaje similar a este:

```
Resumen de pruebas: total: 58, con errores: 0, correcto: 58, omitido: 0, duración: X.X s
Compilación realizado correctamente en X.X s
```

Esto significa que:
- ✅ Todas las pruebas se ejecutaron correctamente
- ✅ No hubo errores
- ✅ El código compiló sin problemas

### Estructura de las Pruebas

Las pruebas están organizadas en las siguientes clases:

1. **AuthServiceTests** - Pruebas del servicio de autenticación
   - Registro de usuarios
   - Inicio de sesión
   - Desbloqueo de cuentas
   - Gestión de usuarios

2. **LigaServiceTests** - Pruebas del servicio de ligas
   - Creación de ligas
   - Actualización de ligas
   - Eliminación de ligas
   - Unirse a ligas

3. **JugadorServiceTests** - Pruebas del servicio de jugadores
   - Creación de jugadores
   - Actualización de jugadores
   - Eliminación de jugadores
   - Búsqueda por equipo y posición

4. **AuthControllerTests** - Pruebas del controlador de autenticación
   - Endpoints de registro
   - Endpoints de login
   - Endpoints de gestión de usuarios

## ⚠️ Qué Hacer si las Pruebas Fallan

### 1. Revisar los Mensajes de Error

Si alguna prueba falla, el comando `dotnet test` mostrará información detallada sobre qué falló. Busca líneas que digan:

```
[X] NombreDeLaPrueba [duración]
   Mensaje de error aquí
```

### 2. Errores Comunes y Soluciones

#### Error: "No se puede encontrar el proyecto"
- **Solución**: Asegúrate de estar en la carpeta `backend/` o especifica la ruta completa al archivo `.sln` o `.csproj`

#### Error: "No se puede restaurar el paquete"
- **Solución**: Ejecuta `dotnet restore` antes de `dotnet test`

#### Error: "Error de compilación"
- **Solución**: 
  1. Ejecuta `dotnet build` para ver los errores de compilación
  2. Corrige los errores en el código
  3. Vuelve a ejecutar las pruebas

#### Error: "No se puede encontrar la referencia"
- **Solución**: Verifica que todas las referencias de proyecto estén correctamente configuradas en el archivo `.csproj` del proyecto de tests

### 3. Limpiar y Reconstruir

Si tienes problemas persistentes, intenta limpiar y reconstruir:

```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
```

## 📁 Estructura del Proyecto de Pruebas

El proyecto de pruebas (`NFLFantasyAPI.Tests`) está organizado de la siguiente manera:

```
backend/src/NFLFantasyAPI.Tests/
├── Logic/
│   └── Services/
│       ├── AuthServiceTests.cs
│       ├── LigaServiceTests.cs
│       └── JugadorServiceTests.cs
└── Presentation/
    └── Controllers/
        └── AuthControllerTests.cs
```

## 🧪 Tecnologías Utilizadas

Las pruebas utilizan:

- **xUnit**: Framework de pruebas para .NET
- **Moq**: Biblioteca para crear objetos mock (simulaciones) de dependencias
- **Microsoft.NET.Test.Sdk**: SDK de pruebas de .NET

## 📝 Notas Importantes

1. **No se usa la base de datos real**: Las pruebas unitarias utilizan mocks (simulaciones) de los repositorios, por lo que NO necesitas tener PostgreSQL corriendo ni ejecutar migraciones.

2. **Pruebas independientes**: Cada prueba es independiente y no depende de otras pruebas.

3. **Cobertura inicial**: Estas son las primeras pruebas unitarias del proyecto. Se pueden agregar más pruebas para aumentar la cobertura del código.


## 📊 Ver Cobertura de Código (Opcional)

Para ver qué porcentaje del código está cubierto por las pruebas, puedes usar herramientas como `coverlet`:

```bash
dotnet add src/NFLFantasyAPI.Tests package coverlet.collector
dotnet test /p:CollectCoverage=true
```

## 🆘 Obtener Ayuda

Si tienes problemas que no se resuelven con esta guía:

1. Revisa los mensajes de error en la consola
2. Verifica que todas las dependencias estén instaladas correctamente
3. Asegúrate de estar usando .NET 9 SDK
4. Consulta la documentación oficial de xUnit: [xunit.net](https://xunit.net/)

---

**Última actualización**: Diciembre 2024  
**Versión de .NET**: 9.0  
**Framework de pruebas**: xUnit 2.x


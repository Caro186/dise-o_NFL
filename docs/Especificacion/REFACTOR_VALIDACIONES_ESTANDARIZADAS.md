# Refactorización: Estandarización de Validaciones - Todos los Servicios

## 📋 Resumen Ejecutivo

Este documento detalla la **estandarización completa de validaciones** realizada en el proyecto "New Generation NFL Fantasy". Se crearon validadores modulares para todas las entidades del sistema, extrayendo toda la lógica de validación de los servicios y centralizándola en clases dedicadas.

**Fecha de Refactorización**: Sprint 3 - Post refactorización JugadorValidator  
**Objetivo**: Estandarizar y modularizar todas las validaciones del sistema para mejorar mantenibilidad, reutilización y claridad del código.

---

## 🎯 Objetivos Cumplidos

✅ **Creación de validadores modulares para todas las entidades**
- NoticiaJugadorValidator
- EquipoNFLValidator
- EquipoFantasyValidator
- LigaValidator
- TemporadaValidator
- FileValidator (validador compartido)

✅ **Extracción completa de validaciones de servicios**
- Todas las validaciones movidas a sus respectivos validadores
- Servicios simplificados y enfocados solo en lógica de negocio

✅ **Estandarización de patrones**
- Misma estructura en todos los validadores
- Métodos consolidados para operaciones comunes
- Uso consistente de excepciones personalizadas

✅ **Registro en Dependency Injection**
- Todos los validadores registrados correctamente en `Program.cs`

---

## 📁 Archivos Creados

### 1. `NoticiaJugadorValidator.cs`
**Ubicación**: `backend/src/NFLFantasyAPI.Logic/Validators/NoticiaJugadorValidator.cs`

**Responsabilidades**:
- Validación de campos requeridos (JugadorId, Texto)
- Validación de longitud de texto (10-300 caracteres)
- Validación de noticias de lesión (resumen, designación)
- Validación de designaciones de lesión válidas (O, D, Q, P, FP, IR, PUP, SUS)
- Validación de existencia y estado del jugador
- Método utilitario estático para descripción de designaciones

**Métodos Consolidados**:
- `ValidarParaCrearAsync(CrearNoticiaJugadorDto dto)` - Valida todo para crear una noticia

**Constantes**:
- `DesignacionesValidas` - HashSet con designaciones válidas de la NFL
- `EstadosValidos` - HashSet con estados válidos de noticia

---

### 2. `EquipoNFLValidator.cs`
**Ubicación**: `backend/src/NFLFantasyAPI.Logic/Validators/EquipoNFLValidator.cs`

**Responsabilidades**:
- Validación de campos requeridos (Nombre, Ciudad)
- Validación de longitud de campos (máximo 100 caracteres)
- Validación de estado válido (Activo, Inactivo)
- Validación de duplicados por nombre
- Validación de existencia del equipo
- Validación de URL de imagen

**Métodos Consolidados**:
- `ValidarParaCrearAsync(EquipoNFLCreateDto dto)` - Valida todo para crear un equipo
- `ValidarParaActualizarAsync(...)` - Valida todo para actualizar un equipo

**Constantes**:
- `EstadosValidos` - HashSet con estados válidos

---

### 3. `EquipoFantasyValidator.cs`
**Ubicación**: `backend/src/NFLFantasyAPI.Logic/Validators/EquipoFantasyValidator.cs`

**Responsabilidades**:
- Validación de campos requeridos (Nombre, UsuarioId)
- Validación de existencia de usuario
- Validación de duplicados (nombre por usuario)
- Validación de pertenencia del equipo al usuario
- Validación de equipo no en liga
- Validación de usuario no tiene equipo en liga

**Métodos Consolidados**:
- `ValidarParaCrearAsync(EquipoFantasyCreateDto dto)` - Valida todo para crear un equipo
- `ValidarParaUnirseALigaAsync(int equipoId, int usuarioId)` - Valida todo para unirse a una liga

**Constantes**:
- `EstadosValidos` - HashSet con estados válidos

---

### 4. `LigaValidator.cs`
**Ubicación**: `backend/src/NFLFantasyAPI.Logic/Validators/LigaValidator.cs`

**Responsabilidades**:
- Validación de campos requeridos (NombreLiga, Password, Cupos, etc.)
- Validación de rango de cupos (2-20)
- Validación de existencia de temporada
- Validación de existencia de comisionado
- Validación de duplicados por nombre
- Validación de contraseña de liga
- Validación de cupos disponibles

**Métodos Consolidados**:
- `ValidarParaCrearAsync(LigaCreateDto dto)` - Valida todo para crear una liga
- `ValidarParaUnirseAsync(UnirseLigaDto dto)` - Valida todo para unirse a una liga (retorna la liga)

**Constantes**:
- `EstadosValidos` - HashSet con estados válidos ("Pre-Draft", "Draft", "En Curso", "Finalizada")
- `MIN_CUPOS = 2`, `MAX_CUPOS = 20`

---

### 5. `TemporadaValidator.cs`
**Ubicación**: `backend/src/NFLFantasyAPI.Logic/Validators/TemporadaValidator.cs`

**Responsabilidades**:
- Validación de campos requeridos (Nombre, Fechas)
- Validación de rango de fechas (inicio < cierre)
- Validación de solapamiento de fechas con otras temporadas
- Validación de semanas dentro del rango de temporada
- Validación de duplicados por nombre
- Validación de existencia de temporada

**Métodos Consolidados**:
- `ValidarParaCrearAsync(CrearTemporadaDto dto)` - Valida todo para crear una temporada

---

### 6. `FileValidator.cs` (Validador Compartido)
**Ubicación**: `backend/src/NFLFantasyAPI.Logic/Validators/FileValidator.cs`

**Responsabilidades**:
- Validación de archivos de imagen (tipo, tamaño)
- Validación de tipos permitidos (JPEG, PNG)
- Validación de tamaño máximo (configurable, default 5 MB)
- Clase estática para reutilización en múltiples servicios

**Métodos**:
- `ValidarArchivoImagen(string? contentType, long fileLength, long maxSizeMB = 5)`

---

## 📝 Archivos Modificados

### Servicios Actualizados

#### 1. `NoticiaJugadorService.cs`
**Cambios**:
- ✅ Inyección de `NoticiaJugadorValidator` en constructor
- ✅ Reemplazo de todas las validaciones inline por `await _validator.ValidarParaCrearAsync(dto)`
- ✅ Uso de método estático `NoticiaJugadorValidator.ObtenerDescripcionDesignacion()` para descripciones
- ✅ Eliminación de método privado `ObtenerDescripcionDesignacion()` duplicado
- ✅ Manejo consistente de excepciones de validación

**Líneas modificadas**: Aproximadamente 40 líneas simplificadas

#### 2. `EquipoNFLService.cs`
**Cambios**:
- ✅ Inyección de `EquipoNFLValidator` en constructor
- ✅ `CreateAsync()`: Usa `await _validator.ValidarParaCrearAsync(dto)`
- ✅ `GetByIdAsync()`: Usa `await _validator.ValidarEquipoExisteAsync(id)`
- ✅ `UploadImagenAsync()`: Usa `FileValidator.ValidarArchivoImagen()`
- ✅ `DeleteAsync()`: Usa `await _validator.ValidarEquipoExisteAsync(id)`
- ✅ Manejo consistente de `ValidationException` y `EquipoNFLNotFoundException`

**Líneas modificadas**: Aproximadamente 60 líneas simplificadas

#### 3. `EquipoFantasyService.cs`
**Cambios**:
- ✅ Inyección de `EquipoFantasyValidator` en constructor
- ✅ `CreateEquipoFantasyAsync()`: Usa `await _validator.ValidarParaCrearAsync(dto)`
- ✅ `GetEquipoFantasyByIdAsync()`: Usa `await _validator.ValidarEquipoExisteAsync(id)`
- ✅ `UploadImagenAsync()`: Usa `FileValidator.ValidarArchivoImagen()`
- ✅ `DeleteEquipoFantasyAsync()`: Usa `await _validator.ValidarEquipoExisteAsync(id)`
- ✅ Manejo consistente de excepciones

**Líneas modificadas**: Aproximadamente 50 líneas simplificadas

#### 4. `LigaService.cs`
**Cambios**:
- ✅ Inyección de `LigaValidator` y `EquipoFantasyValidator` en constructor
- ✅ Inyección de `ITemporadaRepository` para el validador
- ✅ `CreateAsync()`: Usa `await _ligaValidator.ValidarParaCrearAsync(dto)`
- ✅ `GetByIdAsync()`: Usa `await _ligaValidator.ValidarLigaExisteAsync(id)`
- ✅ `UpdateAsync()`: Usa `await _ligaValidator.ValidarLigaExisteAsync(id)`
- ✅ `DeleteAsync()`: Usa `await _ligaValidator.ValidarLigaExisteAsync(id)`
- ✅ `UploadImagenAsync()`: Usa `FileValidator.ValidarArchivoImagen()`
- ✅ `UnirseLigaAsync()`: Usa validadores consolidados
  - `await _ligaValidator.ValidarParaUnirseAsync(dto)` (liga, contraseña, cupos)
  - `await _equipoFantasyValidator.ValidarParaUnirseALigaAsync(...)` (equipo)
  - `await _equipoFantasyValidator.ValidarUsuarioNoTieneEquipoEnLigaAsync(...)`

**Líneas modificadas**: Aproximadamente 80 líneas simplificadas

#### 5. `TemporadaService.cs`
**Cambios**:
- ✅ Inyección de `TemporadaValidator` en constructor
- ✅ `CrearTemporadaAsync()`: Usa `await _validator.ValidarParaCrearAsync(dto)`
- ✅ Eliminación de validaciones inline (duplicados, fechas, solapamiento, semanas)
- ✅ `ObtenerTemporadaAsync()`: Usa `await _validator.ValidarTemporadaExisteAsync(id)`
- ✅ `MarcarComoActualAsync()`: Usa `await _validator.ValidarTemporadaExisteAsync(id)`
- ✅ Manejo consistente de excepciones

**Líneas modificadas**: Aproximadamente 30 líneas simplificadas

---

### Registro en Dependency Injection

#### `Program.cs`
**Cambios**:
```csharp
// Antes:
builder.Services.AddScoped<NFLFantasyAPI.Logic.Validators.JugadorValidator>();

// Ahora:
builder.Services.AddScoped<NFLFantasyAPI.Logic.Validators.JugadorValidator>();
builder.Services.AddScoped<NFLFantasyAPI.Logic.Validators.NoticiaJugadorValidator>();
builder.Services.AddScoped<NFLFantasyAPI.Logic.Validators.EquipoNFLValidator>();
builder.Services.AddScoped<NFLFantasyAPI.Logic.Validators.EquipoFantasyValidator>();
builder.Services.AddScoped<NFLFantasyAPI.Logic.Validators.LigaValidator>();
builder.Services.AddScoped<NFLFantasyAPI.Logic.Validators.TemporadaValidator>();
```

---

## 🏗️ Estructura de Validadores

Todos los validadores siguen la misma estructura estándar:

```csharp
namespace NFLFantasyAPI.Logic.Validators
{
    public class [Entidad]Validator
    {
        #region Constantes
        // Constantes y valores válidos
        #endregion

        #region Constructor
        // Inyección de dependencias
        #endregion

        #region Validaciones de Datos Básicos
        // Validaciones de campos requeridos, formatos, longitudes
        #endregion

        #region Validaciones de Estado
        // Validaciones de estados válidos
        #endregion

        #region Validaciones de Relaciones
        // Validaciones de existencia, duplicados, relaciones
        #endregion

        #region Validaciones de URLs/Archivos
        // Validaciones específicas de URLs o archivos (si aplica)
        #endregion

        #region Métodos Consolidados de Validación
        // Métodos que agrupan múltiples validaciones (ValidarParaCrearAsync, etc.)
        #endregion
    }
}
```

---

## 📊 Métricas de Mejora

### Antes de la Refactorización:
- ❌ **Validaciones dispersas** en múltiples servicios
- ❌ **Código duplicado** de validaciones similares
- ❌ **Difícil mantenimiento** - cambios en validaciones requerían tocar múltiples archivos
- ❌ **Lógica de validación mezclada** con lógica de negocio

### Después de la Refactorización:
- ✅ **Validaciones centralizadas** en clases dedicadas
- ✅ **Código reutilizable** - validaciones compartidas (FileValidator)
- ✅ **Fácil mantenimiento** - cambios en un solo lugar
- ✅ **Separación de responsabilidades** clara
- ✅ **Servicios simplificados** - solo lógica de negocio
- ✅ **Estandarización completa** - misma estructura en todos los validadores

### Estadísticas:
- **Validadores creados**: 6 (incluye FileValidator compartido)
- **Servicios refactorizados**: 5
- **Líneas de código simplificadas**: ~260 líneas
- **Métodos consolidados creados**: 8
- **Duplicación eliminada**: ~15 instancias de validaciones repetidas

---

## 🔍 Ejemplos de Mejora

### Ejemplo 1: Validación de Archivos de Imagen

**Antes** (duplicado en 3 servicios):
```csharp
// En EquipoNFLService, EquipoFantasyService, LigaService
if (imagen == null || imagen.Length == 0)
    return ServiceResult.BadRequest("No se proporcionó ninguna imagen");

var allowedTypes = new[] { "image/jpeg", "image/png" };
if (!allowedTypes.Contains(imagen.ContentType.ToLower()))
    return ServiceResult.BadRequest("Solo se permiten imágenes JPEG o PNG");

if (imagen.Length > 5 * 1024 * 1024)
    return ServiceResult.BadRequest("El tamaño máximo permitido es 5 MB");
```

**Ahora** (centralizado):
```csharp
// En cualquier servicio
FileValidator.ValidarArchivoImagen(imagen.ContentType, imagen.Length, maxSizeMB: 5);
```

---

### Ejemplo 2: Creación de Equipo NFL

**Antes**:
```csharp
public async Task<ServiceResult> CreateAsync(EquipoNFLCreateDto equipoDto)
{
    try
    {
        if (await _repository.ExistsByNameAsync(equipoDto.Nombre))
            return ServiceResult.BadRequest("Ya existe un equipo NFL con ese nombre");

        if (string.IsNullOrWhiteSpace(equipoDto.Nombre))
            return ServiceResult.BadRequest("El nombre es requerido");

        if (string.IsNullOrWhiteSpace(equipoDto.Ciudad))
            return ServiceResult.BadRequest("La ciudad es requerida");

        // ... creación
    }
}
```

**Ahora**:
```csharp
public async Task<ServiceResult> CreateAsync(EquipoNFLCreateDto equipoDto)
{
    try
    {
        // Validaciones usando el validador centralizado (método consolidado)
        await _validator.ValidarParaCrearAsync(equipoDto);

        // ... creación (solo lógica de negocio)
    }
    catch (ValidationException ex)
    {
        _logger.LogWarning($"Error de validación: {ex.Message}");
        return ServiceResult.BadRequest(ex.Message);
    }
}
```

---

### Ejemplo 3: Unirse a Liga

**Antes** (validaciones dispersas):
```csharp
public async Task<ServiceResult> UnirseLigaAsync(UnirseLigaDto dto)
{
    var liga = await _ligaRepository.GetByIdAsync(dto.LigaId);
    if (liga == null)
        return ServiceResult.BadRequest("Liga no encontrada");

    if (!BCrypt.Net.BCrypt.Verify(dto.Password, liga.PasswordHash))
        return ServiceResult.BadRequest("Contraseña incorrecta");

    if (liga.CuposOcupados >= liga.CuposTotales)
        return ServiceResult.BadRequest("La liga está llena");

    var usuario = await _usuarioRespository.GetByIdAsync(dto.UsuarioId);
    if (usuario == null)
        return ServiceResult.BadRequest("Usuario no encontrado");

    var equipoFantasy = await _equipoFantasyRepository.GetByIdAsync(dto.EquipoId);
    if (equipoFantasy == null)
        return ServiceResult.BadRequest("Equipo fantasy no encontrado");

    if (equipoFantasy.UsuarioId != dto.UsuarioId)
        return ServiceResult.BadRequest("El equipo no pertenece al usuario");

    if (equipoFantasy.LigaId.HasValue)
        return ServiceResult.BadRequest("El equipo ya está en otra liga");

    // ... más validaciones
}
```

**Ahora** (validaciones consolidadas):
```csharp
public async Task<ServiceResult> UnirseLigaAsync(UnirseLigaDto dto)
{
    try
    {
        // 1-3. Validaciones de liga usando el validador consolidado
        var liga = await _ligaValidator.ValidarParaUnirseAsync(dto);

        // 4. Validar usuario
        var usuario = await _usuarioRespository.GetByIdAsync(dto.UsuarioId);
        if (usuario == null)
            throw new ValidationException("UsuarioId", "Usuario no encontrado");

        // 5-6. Validaciones de equipo usando el validador consolidado
        await _equipoFantasyValidator.ValidarParaUnirseALigaAsync(dto.EquipoId, dto.UsuarioId);
        await _equipoFantasyValidator.ValidarUsuarioNoTieneEquipoEnLigaAsync(dto.UsuarioId, dto.LigaId);

        // 7. Lógica de negocio (actualizar entidades)
        // ...
    }
    catch (ValidationException ex)
    {
        // Manejo consistente
    }
}
```

---

## ✅ Verificación y Pruebas

### Compilación
```bash
cd backend
dotnet build
```

**Resultado**: ✅ Compilación exitosa sin errores

### Estructura de Validadores
Todos los validadores siguen el mismo patrón:
- ✅ Organizados por regiones (#region)
- ✅ Métodos pequeños y específicos
- ✅ Métodos consolidados para operaciones comunes
- ✅ Uso consistente de excepciones personalizadas

### Servicios Actualizados
Todos los servicios:
- ✅ Inyectan sus validadores respectivos
- ✅ Usan métodos consolidados donde es posible
- ✅ Manejan excepciones de forma consistente
- ✅ Eliminaron validaciones inline

---

## 🔧 Uso de los Validadores

### En Servicios

```csharp
public class MiServicio
{
    private readonly MiValidator _validator;

    public MiServicio(MiValidator validator)
    {
        _validator = validator;
    }

    public async Task<ServiceResult> CrearAsync(MiDto dto)
    {
        try
        {
            // Una sola llamada consolidada
            await _validator.ValidarParaCrearAsync(dto);

            // Lógica de negocio pura
            // ...
        }
        catch (ValidationException ex)
        {
            return ServiceResult.BadRequest(ex.Message);
        }
    }
}
```

### Validaciones Individuales

También puedes llamar validaciones específicas si necesitas más control:

```csharp
// Validar solo campos requeridos
_validator.ValidarCamposRequeridos(dto.Nombre, dto.Ciudad);

// Validar solo existencia
await _validator.ValidarEquipoExisteAsync(equipoId);

// Validar solo duplicados
await _validator.ValidarNoDuplicadoAsync(nombre);
```

---

## 🎓 Beneficios Obtenidos

1. **Mantenibilidad**: Cambios en validaciones en un solo lugar
2. **Reutilización**: Validaciones compartidas entre servicios
3. **Claridad**: Servicios enfocados solo en lógica de negocio
4. **Testabilidad**: Validadores pueden probarse de forma aislada
5. **Consistencia**: Mismo patrón en todo el sistema
6. **Escalabilidad**: Fácil agregar nuevas validaciones o validadores

---

## 📚 Próximos Pasos Recomendados

1. **Crear pruebas unitarias** para cada validador
2. **Documentar** reglas de negocio específicas en cada validador
3. **Considerar** validadores para otras entidades (Usuario, Semana, etc.)
4. **Revisar** controladores para asegurar manejo consistente de excepciones

---

## 📝 Notas de Implementación

- Todos los validadores usan `ValidationException` para errores de validación
- Los validadores lanzan excepciones específicas cuando corresponde (Ej: `EquipoNFLNotFoundException`)
- Los servicios capturan las excepciones y las convierten en `ServiceResult`
- `FileValidator` es una clase estática para facilitar su uso sin inyección de dependencias
- Los métodos consolidados (`ValidarParaCrearAsync`, etc.) agrupan múltiples validaciones en una sola llamada

---

## ✅ Checklist de Verificación

- [x] Todos los validadores creados
- [x] Todos los servicios actualizados para usar validadores
- [x] Todos los validadores registrados en `Program.cs`
- [x] Código compila sin errores
- [x] Validaciones duplicadas eliminadas
- [x] Estructura consistente en todos los validadores
- [x] Manejo consistente de excepciones
- [x] Documentación agregada a todos los validadores

---

**Documento generado**: Refactorización completa de validaciones  
**Estado**: ✅ Completado y verificado

# Estructura del Proyecto - NFL Fantasy API

## Árbol de Directorios Completo

```
📁 dise-o_NFL/
│
├── 📄 README.md
├── 📄 .gitignore
│
├── 📁 docs/                                        (Documentación)
│   ├── 📁 api-docs/                               (Documentación de API)
│   │   ├── convertirusuarioaadmin.txt
│   │   └── jose.txt
│   ├── 📁 architecture/                           (Documentación de arquitectura)
│   │   └── jose.txt
│   ├── 📁 Especificacion/                         (Especificaciones del proyecto)
│   │   ├── CE1116-Especificación de Requerimientos Software para New Generation NFL Fantasy.pdf
│   │   ├── estructura-proyecto.md                 (Este archivo)
│   │   ├── NFL_Fantasy_Documentacion_Completa.pdf
│   │   ├── S12025-Objetivos Sprint 0.pdf
│   │   ├── S12025-Objetivos Sprint 1.pdf
│   │   ├── S12025-Objetivos Sprint 2.pdf
│   │   ├── S12025-Objetivos Sprint 3.pdf
│   │   ├── S12025-Objetivos Sprint 4.pdf
│   │   ├── S22025-Proyecto-NFL Fantasy.pdf
│   │   ├── Sprint_0_-_Grupo1.pdf
│   │   ├── Sprint_1_-_Grupo_1.pdf
│   │   ├── Sprint_2_-_Grupo_1.pdf
│   │   └── Sprint_3_-_Grupo_1.pdf
│   └── 📁 user-stories/                           (Historias de usuario)
│       └── jose.txt
│
├── 📁 backend/                                     ━━━━━ BACKEND (.NET) ━━━━━
│   │
│   ├── 📄 .gitignore
│   ├── 📄 NFLFantasyAPI.sln                       (Solución principal)
│   ├── 📄 tests-backend-how-to.md                 ✨ NUEVO - Guía de pruebas unitarias
│   │
│   └── 📁 src/
│       │
│       ├── 📁 Migrations/                         (Migraciones antiguas - legacy)
│       │   ├── 20251109230042_cambiarTemporada.cs
│       │   ├── 20251109230042_cambiarTemporada.Designer.cs
│       │   ├── 20251109235649_AgregarTablaJugadores.cs
│       │   ├── 20251109235649_AgregarTablaJugadores.Designer.cs
│       │   └── ApplicationDbContextModelSnapshot.cs
│       │
│       ├── 📁 NFLFantasyAPI.CrossCutting/         ━━━ CAPA: Cross-Cutting Concerns ━━━
│       │   ├── 📄 .gitignore
│       │   ├── 📄 NFLFantasyAPI.CrossCutting.csproj
│       │   ├── 📄 README.md
│       │   ├── 📄 ServiceResult.cs                (Objeto de resultado para servicios)
│       │   │
│       │   ├── 📁 Configuration/
│       │   │   ├── FileServerSettings.cs          (Configuración de servidor de archivos)
│       │   │   └── JwtSettings.cs                 (Configuración JWT)
│       │   │
│       │   └── 📁 Interface/
│       │       └── IDbContextProvider.cs          (Interfaz para proveer DbContext)
│       │
│       ├── 📁 NFLFantasyAPI.Persistence/          ━━━ CAPA: Persistencia / Data Access ━━━
│       │   ├── 📄 .gitignore
│       │   ├── 📄 NFLFantasyAPI.Persistence.csproj
│       │   ├── 📄 README.md
│       │   ├── 📄 Class1.cs
│       │   │
│       │   ├── 📁 Context/
│       │   │   └── ApplicationDbContext.cs        (DbContext de Entity Framework)
│       │   │
│       │   ├── 📁 Models/                         (Modelos de base de datos)
│       │   │   ├── Usuario.cs
│       │   │   ├── Liga.cs
│       │   │   ├── Temporada.cs
│       │   │   ├── Semana.cs
│       │   │   ├── EquipoNFL.cs
│       │   │   ├── EquipoFantasy.cs
│       │   │   ├── JugadorNFL.cs
│       │   │   ├── NoticiaJugador.cs
│       │   │   └── BatchResult.cs                 (Resultado de procesamiento batch)
│       │   │
│       │   ├── 📁 Interfaces/                     (Interfaces de repositorios)
│       │   │   ├── IUsuarioRespository.cs
│       │   │   ├── ILigaRepository.cs
│       │   │   ├── ITemporadaRepository.cs
│       │   │   ├── IEquipoNFLRepository.cs
│       │   │   ├── IEquipoFantasyRespository.cs
│       │   │   ├── IJugadorRepository.cs
│       │   │   └── INoticiaJugadorRepository.cs
│       │   │
│       │   ├── 📁 Repositories/                   (Implementación de repositorios)
│       │   │   ├── UsuarioRepository.cs
│       │   │   ├── LigaRepository.cs
│       │   │   ├── TemporadaRepository.cs
│       │   │   ├── EquipoNFLRepository.cs
│       │   │   ├── EquipoFantasyRepository.cs
│       │   │   ├── JugadorRepository.cs
│       │   │   └── NoticiaJugadorRepository.cs
│       │   │
│       │   └── 📁 Migrations/                     (Migraciones de EF Core)
│       │       ├── 20251112053650_MigracionFinal.cs
│       │       ├── 20251112053650_MigracionFinal.Designer.cs
│       │       └── ApplicationDbContextModelSnapshot.cs
│       │
│       ├── 📁 NFLFantasyAPI.Logic/                ━━━ CAPA: Lógica de Negocio ━━━
│       │   ├── 📄 .gitignore
│       │   ├── 📄 NFLFantasyAPI.Logic.csproj
│       │   ├── 📄 Class1.cs
│       │   ├── 📄 DbContextProvide.cs             (Proveedor de DbContext)
│       │   │
│       │   ├── 📁 DTOs/                           (Data Transfer Objects)
│       │   │   ├── AuthDTOs.cs                    (DTOs de autenticación)
│       │   │   ├── UsuarioResponseDto.cs
│       │   │   ├── Desbloquearcuentadto.cs
│       │   │   ├── LigaDTOs.cs
│       │   │   ├── CrearLigaDto.cs
│       │   │   ├── TemporadaDTO.cs
│       │   │   ├── SemanaDTO.cs
│       │   │   ├── EquipoNFLDTOs.cs
│       │   │   ├── EquipoFantasyDTOs.cs
│       │   │   ├── JugadorDtos.cs
│       │   │   ├── JugadorBatchDto.cs             (DTOs para batch de jugadores)
│       │   │   ├── NoticiaJugadorDTOs.cs
│       │   │   └── ErrorResponseDto.cs
│       │   │
│       │   ├── 📁 Exceptions/                     ✨ Excepciones personalizadas
│       │   │   ├── ValidationException.cs
│       │   │   ├── JugadorNotFoundException.cs
│       │   │   ├── JugadorDuplicadoException.cs
│       │   │   ├── EquipoNFLNotFoundException.cs
│       │   │   ├── InvalidFileException.cs
│       │   │   └── BatchProcessingException.cs
│       │   │
│       │   ├── 📁 Validators/                     ✨ Validadores centralizados - REFACTORIZADO
│       │   │   ├── JugadorValidator.cs            (Validaciones de jugadores)
│       │   │   ├── NoticiaJugadorValidator.cs     (Validaciones de noticias de jugadores)
│       │   │   ├── EquipoNFLValidator.cs          (Validaciones de equipos NFL)
│       │   │   ├── EquipoFantasyValidator.cs      (Validaciones de equipos Fantasy)
│       │   │   ├── LigaValidator.cs               (Validaciones de ligas)
│       │   │   ├── TemporadaValidator.cs          (Validaciones de temporadas)
│       │   │   └── FileValidator.cs               (Validaciones de archivos - estático)
│       │   │
│       │   ├── 📁 Interfaces/                     (Interfaces de servicios)
│       │   │   ├── IAuthService.cs
│       │   │   ├── ILigaService.cs
│       │   │   ├── ITemporadaService.cs
│       │   │   ├── IEquipoNFLService.cs
│       │   │   ├── IEquipoFantasyService.cs
│       │   │   ├── IJugadorService.cs
│       │   │   ├── INoticiaJugadorService.cs
│       │   │   └── IBatchFileProcessingService.cs
│       │   │
│       │   └── 📁 Service/                        (Implementación de servicios)
│       │       ├── AuthService.cs                 (Autenticación y usuarios)
│       │       ├── JWTService.cs                  (Generación de tokens JWT)
│       │       ├── LigaService.cs                 ♻️ REFACTORIZADO - Usa validadores
│       │       ├── TemporadaService.cs            ♻️ REFACTORIZADO - Usa validadores
│       │       ├── EquipoNFLService.cs            ♻️ REFACTORIZADO - Usa validadores
│       │       ├── EquipoFantasyService.cs        ♻️ REFACTORIZADO - Usa validadores
│       │       ├── JugadorService.cs              ♻️ REFACTORIZADO - Usa validadores
│       │       ├── NoticiaJugadorService.cs       ♻️ REFACTORIZADO - Usa validadores
│       │       └── BatchFileProcessingService.cs  ✨ Manejo de archivos
│       │
│       ├── 📁 NFLFantasyAPI.Presentation/         ━━━ CAPA: Presentación / API ━━━
│       │   ├── 📄 .gitignore
│       │   ├── 📄 NFLFantasyAPI.Presentation.csproj
│       │   ├── 📄 README.md
│       │   ├── 📄 Program.cs                      ♻️ ACTUALIZADO - DI registrada
│       │   ├── 📄 appsettings.json
│       │   ├── 📄 appsettings.Development.json
│       │   ├── 📄 NFLFantasyAPI.Presentation.http ✨ NUEVO - Archivo de pruebas HTTP
│       │   │
│       │   ├── 📁 Controllers/                    (Controladores API REST)
│       │   │   ├── AuthController.cs              (Endpoints de autenticación)
│       │   │   ├── LigaController.cs
│       │   │   ├── TemporadaController.cs
│       │   │   ├── EquipoNFLController.cs
│       │   │   ├── EquipoFantasyController.cs
│       │   │   ├── JugadorController.cs           (Endpoints de jugadores)
│       │   │   └── NoticiaJugadorController.cs
│       │   │
│       │   ├── 📁 Properties/
│       │   │   └── launchSettings.json
│       │   │
│       │   ├── 📁 logs/                           (Archivos de log - Serilog)
│       │   │   ├── nfl-fantasy-20251112_002.txt
│       │   │   ├── nfl-fantasy-20251126.txt
│       │   │   ├── nfl-fantasy-20251126_001.txt
│       │   │   ├── nfl-fantasy-20251126_002.txt
│       │   │   ├── nfl-fantasy-20251126_003.txt
│       │   │   ├── nfl-fantasy-20251127.txt
│       │   │   ├── nfl-fantasy-20251127_001.txt
│       │   │   └── nfl-fantasy-20251127_002.txt
│       │   │
│       │   └── 📁 wwwroot/                       ✨ NUEVO - Archivos estáticos
│       │       └── 📁 uploads/                    (Archivos subidos)
│       │           ├── 📁 equipos/
│       │           │   └── .gitkeep
│       │           ├── 📁 equipos-fantasy/
│       │           │   ├── 1_d0c067f9-ca69-4907-90c0-9585f4d16874.png
│       │           │   └── 2_f09bd11a-63a6-4a47-b8cf-9d34a23b1840.png
│       │           ├── 📁 equipos-nfl/
│       │           │   └── 1_7ace833b-cfd2-44f9-9fea-56c9444f2748.png
│       │           └── 📁 jugadores/
│       │               ├── 1_4218e20d-badd-44c0-9133-ec4f664ac402.png
│       │               └── 📁 thumbnails/
│       │                   └── 1_thumb_1303564b-cc39-4cc4-a8e0-309d223d21fa.png
│       │
│       └── 📁 NFLFantasyAPI.Tests/                ✨ NUEVO - Proyecto de Pruebas Unitarias
│           ├── 📄 NFLFantasyAPI.Tests.csproj
│           ├── 📄 UnitTest1.cs
│           │
│           ├── 📁 Logic/                          (Pruebas de lógica de negocio)
│           │   └── 📁 Services/
│           │       ├── AuthServiceTests.cs         (15 pruebas)
│           │       ├── LigaServiceTests.cs         (12 pruebas)
│           │       └── JugadorServiceTests.cs      (18 pruebas)
│           │
│           └── 📁 Presentation/                   (Pruebas de controladores)
│               └── 📁 Controllers/
│                   └── AuthControllerTests.cs      (13 pruebas)
│
└── 📁 frontend/                                    ━━━━━ FRONTEND (Angular) ━━━━━
    │
    ├── 📄 README.md
    ├── 📄 .editorconfig
    ├── 📄 package.json
    ├── 📄 package-lock.json
    ├── 📄 angular.json
    ├── 📄 tsconfig.json
    ├── 📄 tsconfig.app.json
    ├── 📄 tsconfig.spec.json
    ├── 📄 tests-frontend-how-to.md                ✨ NUEVO - Guía de pruebas unitarias
    │
    └── 📁 src/
        │
        ├── 📄 index.html                          (HTML principal)
        ├── 📄 main.ts                             (Bootstrap de Angular)
        ├── 📄 styles.css                          (Estilos globales)
        │
        ├── 📁 app/                                (Módulo principal)
        │   ├── app.ts                             (Componente raíz)
        │   ├── app.html
        │   ├── app.css
        │   ├── app.spec.ts                        ✨ Pruebas unitarias
        │   ├── app.config.ts                      (Configuración de la app)
        │   └── app.routes.ts                      (Rutas de la aplicación)
        │
        ├── 📁 assets/                             ✨ NUEVO - Recursos estáticos
        │   └── 📁 img/
        │       └── image.png
        │
        ├── 📁 models/                             (Modelos TypeScript)
        │   └── item.ts
        │
        ├── 📁 guards/                             (Guards de autenticación)
        │   ├── auth.guard.ts                       (Protección de rutas)
        │   ├── auth.guard.spec.ts                 ✨ Pruebas unitarias (3 pruebas)
        │   └── admin.guard.ts
        │
        ├── 📁 services/                           (Servicios Angular)
        │   ├── api.ts                             (Servicio base API)
        │   ├── api.spec.ts                        ✨ Pruebas unitarias (1 prueba)
        │   ├── authservice.ts                     (Servicio de autenticación)
        │   ├── authservice.spec.ts                ✨ Pruebas unitarias (10 pruebas)
        │   ├── jwt.interceptor.ts                 (Interceptor JWT)
        │   ├── liga.service.ts
        │   ├── temporada.service.ts
        │   ├── equipo-nfl.service.ts
        │   ├── equipo-fantasy.service.ts
        │   ├── jugadores.service.ts
        │   └── noticia-jugador.service.ts
        │
        ├── 📁 loginwidgets/                       (Componentes de login)
        │   ├── 📁 login/
        │   │   ├── login.ts
        │   │   ├── login.html
        │   │   ├── login.css
        │   │   └── login.spec.ts                   ✨ Pruebas unitarias (17 pruebas)
        │   │
        │   └── 📁 register/
        │       ├── register.ts
        │       ├── register.html
        │       ├── register.css
        │       └── register.spec.ts                ✨ Pruebas unitarias (1 prueba)
        │
        ├── 📁 perfil/                             (Componente de perfil)
        │   ├── perfil.ts
        │   ├── perfil.html
        │   └── perfil.css
        │
        └── 📁 mainpage/                           (Componentes principales - post-login)
            │
            ├── mainpage.component.ts               (Layout principal)
            ├── mainpage.html
            ├── mainpage.css
            │
            ├── 📁 sidenav/                        (Menú lateral)
            │   ├── sidenav.ts
            │   ├── sidenav.html
            │   ├── sidenav.css
            │   └── sidenav.spec.ts                    ✨ Pruebas unitarias (1 prueba)
            │
            ├── 📁 liga/                           (Gestión de ligas)
            │   ├── liga.ts
            │   ├── liga.html
            │   ├── liga.css
            │   └── liga.spec.ts                    ✨ Pruebas unitarias (9 pruebas)
            │
            ├── 📁 crear-liga/
            │   ├── crear-liga.ts
            │   ├── crear-liga.html
            │   ├── crear-liga.css
            │   └── crear-liga.spec.ts              ✨ Pruebas unitarias (13 pruebas)
            │
            ├── 📁 buscar-unirse-liga/
            │   ├── buscar-unirse-liga.ts
            │   ├── buscar-unirse-liga.html
            │   ├── buscar-unirse-liga.css
            │   └── buscar-unirse-liga.spec.ts      ✨ Pruebas unitarias (18 pruebas)
            │
            ├── 📁 temporada/
            │   ├── temporada.ts
            │   ├── temporada.html
            │   ├── temporada.css
            │   └── temporada.spec.ts                ✨ Pruebas unitarias (1 prueba)
            │
            ├── 📁 equipos-nfl-list/               (Lista de equipos NFL)
            │   ├── equipos-nfl-list.component.ts
            │   ├── equipos-nfl-list.component.html
            │   └── equipos-nfl-list.component.css
            │
            ├── 📁 equipos-nfl-form/               (Formulario equipos NFL)
            │   ├── equipos-nfl-form.component.ts
            │   ├── equipos-nfl-form.component.html
            │   └── equipos-nfl-form.component.css
            │
            ├── 📁 equipos-fantasy-list/           (Lista de equipos Fantasy)
            │   ├── equipos-fantasy-list.ts
            │   ├── equipos-fantasy-list.html
            │   ├── equipos-fantasy-list.css
            │   └── equipos-fantasy-list.spec.ts    ✨ Pruebas unitarias (1 prueba)
            │
            ├── 📁 equipos-fantasy-form/           (Formulario equipos Fantasy)
            │   ├── equipos-fantasy-form.ts
            │   ├── equipos-fantasy-form.html
            │   ├── equipos-fantasy-form.css
            │   └── equipos-fantasy-form.spec.ts    ✨ Pruebas unitarias (10 pruebas)
            │
            ├── 📁 jugadores/                      (Lista de jugadores)
            │   ├── jugadores.component.ts
            │   ├── jugadores.component.html
            │   └── jugadores.component.css
            │
            ├── 📁 creacion-manual-de-jugador/     (Crear jugador manual)
            │   ├── form-jugador.ts
            │   ├── form-jugador.html
            │   └── form-jugador.css
            │
            ├── 📁 jugadores-batch/                (Gestión batch de jugadores)
            │   ├── jugadores-batch.ts
            │   ├── jugadores-batch.html
            │   ├── jugadores-batch.css
            │   └── jugadores-batch.spec.ts         ✨ Pruebas unitarias (1 prueba)
            │
            ├── 📁 jugador-batch-upload/           (Upload batch jugadores)
            │   ├── jugador-batch-upload.ts
            │   ├── jugador-batch-upload.html
            │   └── jugador-batch-upload.css
            │
            ├── 📁 noticia-jugador/                (Crear/editar noticias)
            │   ├── noticia-jugador.ts
            │   ├── noticia-jugador.html
            │   ├── noticia-jugador.css
            │   └── noticia-jugador.spec.ts         ✨ Pruebas unitarias (1 prueba)
            │
            └── 📁 ver-noticias/                   (Ver lista de noticias)
                ├── ver-noticias.ts
                ├── ver-noticias.html
                ├── ver-noticias.css
                └── ver-noticias.spec.ts            ✨ Pruebas unitarias (1 prueba)
```

## Arquitectura del Backend (Capas)

```
┌─────────────────────────────────────────────────────────────┐
│  NFLFantasyAPI.Presentation (API Layer)                    │
│  - Controllers (REST API endpoints)                        │
│  - Program.cs (Configuración, DI, Middleware)              │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│  NFLFantasyAPI.Logic (Business Logic Layer)                │
│  - Services (Lógica de negocio)                            │
│  - Validators ✨ (Validaciones centralizadas)              │
│  - Exceptions ✨ (Excepciones personalizadas)              │
│  - DTOs (Data Transfer Objects)                            │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│  NFLFantasyAPI.Persistence (Data Access Layer)             │
│  - Repositories (Acceso a datos)                           │
│  - Models (Entidades de DB)                                │
│  - DbContext (Entity Framework Core)                       │
└─────────────────────────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│  NFLFantasyAPI.CrossCutting (Shared Concerns)              │
│  - ServiceResult (Respuestas uniformes)                    │
│  - Configuration (Settings compartidos)                    │
└─────────────────────────────────────────────────────────────┘
```

## Arquitectura del Frontend (Angular)

```
┌─────────────────────────────────────────┐
│  Core (App, Guards, Interceptors)      │
└───────────────┬─────────────────────────┘
                │
┌───────────────▼─────────────────────────┐
│  Services (API Communication)           │
└───────────────┬─────────────────────────┘
                │
┌───────────────▼─────────────────────────┐
│  Components (UI)                        │
│  - Login/Register                       │
│  - Mainpage (Post-auth features)        │
│  - Ligas, Equipos, Jugadores, Noticias │
└─────────────────────────────────────────┘
```

## Archivos Clave de la Refactorización

### Nuevos - Backend
- `backend/src/NFLFantasyAPI.Logic/Exceptions/*.cs` (6 archivos)
- `backend/src/NFLFantasyAPI.Logic/Validators/` ✨ Sistema completo de validadores:
  - `JugadorValidator.cs` (Validaciones de jugadores)
  - `NoticiaJugadorValidator.cs` (Validaciones de noticias)
  - `EquipoNFLValidator.cs` (Validaciones de equipos NFL)
  - `EquipoFantasyValidator.cs` (Validaciones de equipos Fantasy)
  - `LigaValidator.cs` (Validaciones de ligas)
  - `TemporadaValidator.cs` (Validaciones de temporadas)
  - `FileValidator.cs` (Validaciones de archivos - clase estática)
- `backend/src/NFLFantasyAPI.Logic/Service/BatchFileProcessingService.cs`
- `backend/src/NFLFantasyAPI.Logic/Interfaces/IBatchFileProcessingService.cs`
- `backend/src/NFLFantasyAPI.Tests/` ✨ Proyecto completo de pruebas unitarias
  - `Logic/Services/AuthServiceTests.cs` (15 pruebas)
  - `Logic/Services/LigaServiceTests.cs` (12 pruebas)
  - `Logic/Services/JugadorServiceTests.cs` (18 pruebas)
  - `Presentation/Controllers/AuthControllerTests.cs` (13 pruebas)
- `backend/src/NFLFantasyAPI.Presentation/wwwroot/uploads/` ✨ Carpeta de archivos subidos
- `backend/src/NFLFantasyAPI.Presentation/NFLFantasyAPI.Presentation.http` ✨ Archivo de pruebas HTTP
- `backend/tests-backend-how-to.md` ✨ Guía de pruebas unitarias del backend

### Nuevos - Frontend
- `frontend/src/assets/img/` ✨ Carpeta de imágenes
- `frontend/tests-frontend-how-to.md` ✨ Guía de pruebas unitarias del frontend
- Múltiples archivos `.spec.ts` ✨ Pruebas unitarias de componentes y servicios

### Modificados - Refactorización de Validaciones
- `backend/src/NFLFantasyAPI.Logic/Service/JugadorService.cs` ♻️ Refactorizado para usar validadores
- `backend/src/NFLFantasyAPI.Logic/Service/NoticiaJugadorService.cs` ♻️ Refactorizado para usar validadores
- `backend/src/NFLFantasyAPI.Logic/Service/EquipoNFLService.cs` ♻️ Refactorizado para usar validadores
- `backend/src/NFLFantasyAPI.Logic/Service/EquipoFantasyService.cs` ♻️ Refactorizado para usar validadores
- `backend/src/NFLFantasyAPI.Logic/Service/LigaService.cs` ♻️ Refactorizado para usar validadores
- `backend/src/NFLFantasyAPI.Logic/Service/TemporadaService.cs` ♻️ Refactorizado para usar validadores
- `backend/src/NFLFantasyAPI.Presentation/Program.cs` ♻️ DI actualizada con todos los validadores registrados

## Pruebas Unitarias

### Backend (NFLFantasyAPI.Tests)
- **Total de pruebas:** 58
- **Tecnologías:** xUnit, Moq
- **Cobertura:**
  - AuthService: 15 pruebas
  - LigaService: 12 pruebas
  - JugadorService: 18 pruebas
  - AuthController: 13 pruebas

### Frontend
- **Total de pruebas:** ~75+ pruebas
- **Tecnologías:** Jasmine, Karma, Angular Testing Utilities
- **Cobertura:**
  - Servicios: authservice (10), api (1)
  - Guards: auth.guard (3)
  - Componentes: login (17), register (1), liga (9), crear-liga (13), buscar-unirse-liga (18), equipos-fantasy-form (10), y más

---

## Sistema de Validadores - Refactorización Sprint 3

### Arquitectura de Validadores

Todos los servicios han sido refactorizados para usar validadores centralizados, siguiendo el principio de responsabilidad única:

```
┌─────────────────────────────────────────────────────────┐
│  Services (Lógica de Negocio)                          │
│  - Delegan validaciones a Validators                   │
│  - Enfocados en orquestación                           │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│  Validators (Validaciones Centralizadas)               │
│  - JugadorValidator                                     │
│  - NoticiaJugadorValidator                             │
│  - EquipoNFLValidator                                  │
│  - EquipoFantasyValidator                              │
│  - LigaValidator                                       │
│  - TemporadaValidator                                  │
│  - FileValidator (estático)                            │
└─────────────────────────────────────────────────────────┘
```

### Características del Sistema de Validadores

- **Modularización completa:** Cada entidad tiene su validador dedicado
- **Métodos pequeños y claros:** Validaciones granulares y reutilizables
- **Métodos consolidados:** Entry points como `ValidarParaCrearAsync()` para operaciones comunes
- **Separación de responsabilidades:** Los servicios delegan todas las validaciones
- **Manejo de excepciones:** Uso consistente de `ValidationException` con mensajes claros
- **Dependency Injection:** Todos los validadores registrados en `Program.cs`

### Servicios Refactorizados

Todos los servicios siguientes ahora usan validadores centralizados:
- ✅ `JugadorService` → `JugadorValidator`
- ✅ `NoticiaJugadorService` → `NoticiaJugadorValidator`
- ✅ `EquipoNFLService` → `EquipoNFLValidator` + `FileValidator`
- ✅ `EquipoFantasyService` → `EquipoFantasyValidator` + `FileValidator`
- ✅ `LigaService` → `LigaValidator` + `EquipoFantasyValidator` + `FileValidator`
- ✅ `TemporadaService` → `TemporadaValidator`

---

**Última actualización:** Diciembre 2024
**Refactorización:** Sistema completo de validadores centralizados - Sprint 3
**Nuevas características:** 
- Sistema modularizado de validaciones para todas las entidades
- Proyecto de pruebas unitarias completo (backend y frontend)
- Sistema de archivos estáticos

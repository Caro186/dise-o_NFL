# Guía de Pruebas Unitarias del Frontend - NFL Fantasy API

Esta guía explica cómo ejecutar las pruebas unitarias del frontend del proyecto "New Generation NFL Fantasy".

## 📋 Requisitos Previos

Antes de ejecutar las pruebas, asegúrate de tener instalado:

- **Node.js** (versión 18 o superior)
  - Puedes verificar tu versión ejecutando: `node --version`
  - Si no lo tienes, descárgalo desde [nodejs.org](https://nodejs.org/)

- **npm** (viene con Node.js)
  - Puedes verificar tu versión ejecutando: `npm --version`

- **Dependencias del proyecto instaladas**
  - Ejecuta `npm install` en la carpeta `frontend/` si aún no lo has hecho

## 🚀 Cómo Ejecutar las Pruebas

### Ejecutar Todas las Pruebas

Abre una terminal y navega a la carpeta `frontend/`:

```bash
cd frontend
npm test
```

Este comando ejecutará todas las pruebas unitarias y abrirá un navegador (Chrome) donde podrás ver los resultados.

### Ejecutar Pruebas sin Abrir Navegador (Headless)

Para ejecutar las pruebas sin abrir el navegador (útil para CI/CD):

```bash
npm test -- --watch=false --browsers=ChromeHeadless
```

### Ejecutar Pruebas en Modo Watch

Para ejecutar las pruebas en modo watch (se re-ejecutan automáticamente cuando cambias archivos):

```bash
npm test -- --watch
```

### Ejecutar Pruebas Específicas

**⚠️ Importante:** Todos los archivos usan `fdescribe` por defecto. Para ejecutar un archivo específico, asegúrate de que solo ese archivo tenga `fdescribe` activo (cambia los demás a `xdescribe` temporalmente).

#### Servicios

**authservice.spec.ts** (10 pruebas): Pruebas del servicio de autenticación (login, registro, logout, sesión).

```bash
cd "C:\Obsidian\OrganizaciónJoseAndresVargasTorres\02-Studies\Product Design and Quality\Proyect\dise-o_NFL\frontend" && npm test -- --watch=false --browsers=ChromeHeadless
```

**api.spec.ts** (1 prueba): Pruebas básicas del servicio de API.

```bash
cd "C:\Obsidian\OrganizaciónJoseAndresVargasTorres\02-Studies\Product Design and Quality\Proyect\dise-o_NFL\frontend" && npm test -- --watch=false --browsers=ChromeHeadless
```

#### Guards

**auth.guard.spec.ts** (3 pruebas): Pruebas del guard de autenticación (permite/deniega acceso según login).

```bash
cd "C:\Obsidian\OrganizaciónJoseAndresVargasTorres\02-Studies\Product Design and Quality\Proyect\dise-o_NFL\frontend" && npm test -- --watch=false --browsers=ChromeHeadless
```

#### Componentes de Login

**login.spec.ts** (17 pruebas): Pruebas del componente de login (validaciones, envío, errores).

```bash
cd "C:\Obsidian\OrganizaciónJoseAndresVargasTorres\02-Studies\Product Design and Quality\Proyect\dise-o_NFL\frontend" && npm test -- --watch=false --browsers=ChromeHeadless
```

**register.spec.ts** (1 prueba): Pruebas básicas del componente de registro.

```bash
cd "C:\Obsidian\OrganizaciónJoseAndresVargasTorres\02-Studies\Product Design and Quality\Proyect\dise-o_NFL\frontend" && npm test -- --watch=false --browsers=ChromeHeadless
```

#### Componentes de Ligas

**liga.spec.ts** (9 pruebas): Pruebas del componente de lista de ligas (carga, errores, utilidades).

```bash
cd "C:\Obsidian\OrganizaciónJoseAndresVargasTorres\02-Studies\Product Design and Quality\Proyect\dise-o_NFL\frontend" && npm test -- --watch=false --browsers=ChromeHeadless
```

**crear-liga.spec.ts** (13 pruebas): Pruebas del componente de crear liga (validaciones, creación con/sin equipo).

```bash
cd "C:\Obsidian\OrganizaciónJoseAndresVargasTorres\02-Studies\Product Design and Quality\Proyect\dise-o_NFL\frontend" && npm test -- --watch=false --browsers=ChromeHeadless
```

**buscar-unirse-liga.spec.ts** (18 pruebas): Pruebas del componente de buscar y unirse a liga (búsqueda, validaciones, unión).

```bash
cd "C:\Obsidian\OrganizaciónJoseAndresVargasTorres\02-Studies\Product Design and Quality\Proyect\dise-o_NFL\frontend" && npm test -- --watch=false --browsers=ChromeHeadless
```

#### Componentes de Equipos

**equipos-fantasy-form.spec.ts** (10 pruebas): Pruebas del formulario de equipos fantasy (validaciones, creación, imagen).

```bash
cd "C:\Obsidian\OrganizaciónJoseAndresVargasTorres\02-Studies\Product Design and Quality\Proyect\dise-o_NFL\frontend" && npm test -- --watch=false --browsers=ChromeHeadless
```

**equipos-fantasy-list.spec.ts** (1 prueba): Pruebas básicas del componente de lista de equipos fantasy.

```bash
cd "C:\Obsidian\OrganizaciónJoseAndresVargasTorres\02-Studies\Product Design and Quality\Proyect\dise-o_NFL\frontend" && npm test -- --watch=false --browsers=ChromeHeadless
```

#### Otros Componentes

**sidenav.spec.ts** (1 prueba): Pruebas básicas del componente de navegación lateral.

```bash
cd "C:\Obsidian\OrganizaciónJoseAndresVargasTorres\02-Studies\Product Design and Quality\Proyect\dise-o_NFL\frontend" && npm test -- --watch=false --browsers=ChromeHeadless
```

**temporada.spec.ts** (1 prueba): Pruebas básicas del componente de temporadas.

```bash
cd "C:\Obsidian\OrganizaciónJoseAndresVargasTorres\02-Studies\Product Design and Quality\Proyect\dise-o_NFL\frontend" && npm test -- --watch=false --browsers=ChromeHeadless
```

**noticia-jugador.spec.ts** (1 prueba): Pruebas básicas del componente de noticias de jugador.

```bash
cd "C:\Obsidian\OrganizaciónJoseAndresVargasTorres\02-Studies\Product Design and Quality\Proyect\dise-o_NFL\frontend" && npm test -- --watch=false --browsers=ChromeHeadless
```

**ver-noticias.spec.ts** (1 prueba): Pruebas básicas del componente de ver noticias.

```bash
cd "C:\Obsidian\OrganizaciónJoseAndresVargasTorres\02-Studies\Product Design and Quality\Proyect\dise-o_NFL\frontend" && npm test -- --watch=false --browsers=ChromeHeadless
```

**jugadores-batch.spec.ts** (1 prueba): Pruebas básicas del componente de carga batch de jugadores.

```bash
cd "C:\Obsidian\OrganizaciónJoseAndresVargasTorres\02-Studies\Product Design and Quality\Proyect\dise-o_NFL\frontend" && npm test -- --watch=false --browsers=ChromeHeadless
```

**app.spec.ts** (2 pruebas): Pruebas básicas del componente principal de la aplicación.

```bash
cd "C:\Obsidian\OrganizaciónJoseAndresVargasTorres\02-Studies\Product Design and Quality\Proyect\dise-o_NFL\frontend" && npm test -- --watch=false --browsers=ChromeHeadless
```

## ✅ Resultados Esperados

### Cuando las Pruebas Pasan Correctamente

Deberías ver en la consola y en el navegador un mensaje similar a:

```
Chrome Headless: Executed X of Y SUCCESS
```

Y en el navegador verás:
- ✅ Lista de todas las pruebas ejecutadas
- ✅ Contador de pruebas pasadas/fallidas
- ✅ Tiempo de ejecución

### Estructura de las Pruebas

Las pruebas están organizadas en las siguientes categorías:

1. **Servicios** (`src/services/`)
   - `authservice.spec.ts` - Pruebas del servicio de autenticación
   - `api.spec.ts` - Pruebas del servicio de API

2. **Guards** (`src/guards/`)
   - `auth.guard.spec.ts` - Pruebas del guard de autenticación

3. **Componentes de Login** (`src/loginwidgets/`)
   - `login/login.spec.ts` - Pruebas del componente de login
   - `register/register.spec.ts` - Pruebas del componente de registro

4. **Componentes de Ligas** (`src/mainpage/`)
   - `liga/liga.spec.ts` - Pruebas del componente de lista de ligas
   - `crear-liga/crear-liga.spec.ts` - Pruebas del componente de crear liga
   - `buscar-unirse-liga/buscar-unirse-liga.spec.ts` - Pruebas del componente de buscar y unirse a liga

5. **Componentes de Equipos** (`src/mainpage/`)
   - `equipos-fantasy-form/equipos-fantasy-form.spec.ts` - Pruebas del formulario de equipos fantasy

## ⚠️ Qué Hacer si las Pruebas Fallan

### 1. Revisar los Mensajes de Error

Si alguna prueba falla, el navegador y la consola mostrarán información detallada:
- Nombre de la prueba que falló
- Mensaje de error específico
- Stack trace del error

### 2. Errores Comunes y Soluciones

#### Error: "Could not find the '@angular/build:karma' builder"
- **Solución**: Ejecuta `npm install` para instalar todas las dependencias

#### Error: "Node packages may not be installed"
- **Solución**: Ejecuta `npm install` en la carpeta `frontend/`

#### Error: "Cannot find module"
- **Solución**: 
  1. Verifica que todas las dependencias estén instaladas: `npm install`
  2. Verifica que los imports en los archivos `.spec.ts` sean correctos
  3. Asegúrate de que los archivos fuente existan

#### Error: "TypeError: Cannot read property 'X' of undefined"
- **Solución**: Verifica que los mocks estén correctamente configurados en el test

#### Error: "Expected X to have been called but it was not"
- **Solución**: Verifica que el método mockeado se esté llamando correctamente en el código

### 3. Limpiar y Reinstalar

Si tienes problemas persistentes, intenta limpiar y reinstalar:

```bash
# Eliminar node_modules y package-lock.json
rm -rf node_modules package-lock.json

# Reinstalar dependencias
npm install

# Ejecutar pruebas nuevamente
npm test
```

En Windows PowerShell:
```powershell
Remove-Item -Recurse -Force node_modules, package-lock.json
npm install
npm test
```

## 📁 Estructura del Proyecto de Pruebas

Los archivos de pruebas (`.spec.ts`) están ubicados junto a los archivos fuente que prueban:

```
frontend/src/
├── services/
│   ├── authservice.ts
│   └── authservice.spec.ts
├── guards/
│   ├── auth.guard.ts
│   └── auth.guard.spec.ts
├── loginwidgets/
│   └── login/
│       ├── login.ts
│       └── login.spec.ts
└── mainpage/
    ├── liga/
    │   ├── liga.ts
    │   └── liga.spec.ts
    ├── crear-liga/
    │   ├── crear-liga.ts
    │   └── crear-liga.spec.ts
    └── buscar-unirse-liga/
        ├── buscar-unirse-liga.ts
        └── buscar-unirse-liga.spec.ts
```

## 🧪 Tecnologías Utilizadas

Las pruebas utilizan:

- **Jasmine**: Framework de pruebas para JavaScript/TypeScript
- **Karma**: Test runner que ejecuta las pruebas en navegadores reales
- **Angular Testing Utilities**: Utilidades de Angular para testing (`TestBed`, `ComponentFixture`, etc.)
- **HttpClientTestingModule**: Módulo para testear servicios HTTP
- **RouterTestingModule**: Módulo para testear componentes con routing

## 📝 Notas Importantes

1. **No se usa el backend real**: Las pruebas utilizan mocks (simulaciones) de los servicios HTTP, por lo que NO necesitas tener el backend corriendo.

2. **Pruebas independientes**: Cada prueba es independiente y no depende de otras pruebas.

3. **Cobertura inicial**: Estas son las primeras pruebas unitarias del frontend. Se pueden agregar más pruebas para aumentar la cobertura del código.

4. **Mocks y Spies**: Los tests utilizan Jasmine spies para simular el comportamiento de servicios y dependencias.


## 📊 Ver Cobertura de Código (Opcional)

Para ver qué porcentaje del código está cubierto por las pruebas, puedes usar:

```bash
npm test -- --code-coverage
```

Esto generará un reporte de cobertura en la carpeta `coverage/`. Abre `coverage/index.html` en tu navegador para ver el reporte detallado.

## 🆘 Obtener Ayuda

Si tienes problemas que no se resuelven con esta guía:

1. Revisa los mensajes de error en la consola y en el navegador
2. Verifica que todas las dependencias estén instaladas correctamente
3. Asegúrate de estar usando Node.js 18 o superior
4. Consulta la documentación oficial de Angular Testing: [angular.io/guide/testing](https://angular.io/guide/testing)
5. Consulta la documentación de Jasmine: [jasmine.github.io](https://jasmine.github.io/)

---

**Última actualización**: Diciembre 2024  
**Versión de Angular**: 20.3  
**Framework de pruebas**: Jasmine + Karma


# Instrucciones para Recrear el Diagrama C2 en Draw.io / Lucidchart

## Estructura del Diagrama

### Layout General
- **4 columnas principales**: Desarrollo | QA | Staging | Producción
- **8 capas horizontales** (de arriba a abajo):
  1. Network & Security
  2. Application Gateway & Load Balancing
  3. Compute - Application Services
  4. Data Storage
  5. Caching & Messaging
  6. CDN & Content Delivery
  7. Security & Secrets Management
  8. Monitoring & Logging

### Elementos Visuales Recomendados

#### Formas y Colores
- **VNet**: Rectángulo con borde grueso, color azul claro
- **Subnets**: Rectángulos dentro de VNet, color azul más claro
- **App Services**: Rectángulos redondeados, color verde
- **Databases**: Cilindros, color naranja
- **Storage**: Rectángulos con icono de disco, color gris
- **Load Balancer**: Rectángulo con icono de balanza, color morado
- **Security**: Escudos, color rojo
- **Monitoring**: Ojos/gráficos, color amarillo

#### Conectores
- **Líneas sólidas**: Comunicación directa
- **Líneas punteadas**: Comunicación opcional/backup
- **Flechas**: Dirección del flujo de datos

## Pasos para Crear en Draw.io

1. **Crear el Canvas**
   - Tamaño: A3 o Personalizado (1600x2400px)
   - Grid habilitado para alineación

2. **Dibujar las Columnas de Ambientes**
   - Crear 4 rectángulos grandes verticales
   - Etiquetar: Desarrollo, QA, Staging, Producción
   - Color de fondo: Gris muy claro (#F5F5F5)

3. **Layer 1: Network & Security**
   - Para cada ambiente:
     - VNet (rectángulo grande)
     - 3 Subnets dentro (Frontend, App, Database)
     - NSG (escudo pequeño)
     - DDoS Protection solo en Producción

4. **Layer 2: Application Gateway**
   - Application Gateway (rectángulo con icono de puerta)
   - Load Balancer interno (rectángulo con icono de balanza)
   - Conectar con flechas hacia abajo

5. **Layer 3: Compute**
   - API Gateway (App Service)
   - Microservicios (Auth, Jugador, Liga, Temporada)
   - Cada uno como App Service
   - Agrupar por tipo de servicio

6. **Layer 4: Data Storage**
   - SQL Database (cilindro)
   - Blob Storage (rectángulo con icono de disco)
   - Cosmos DB (solo Staging y Prod)

7. **Layer 5: Caching & Messaging**
   - Redis Cache (rectángulo con icono de memoria)
   - Service Bus (rectángulo con icono de mensaje)

8. **Layer 6: CDN**
   - Azure CDN (nube con icono de CDN)

9. **Layer 7: Security**
   - Key Vault (caja fuerte)

10. **Layer 8: Monitoring**
    - Application Insights (ojo/gráfico)
    - Log Analytics (documento)
    - Azure Monitor (gráfico de barras)

11. **Conectar Componentes**
    - Application Gateway → Load Balancer → App Services
    - App Services → SQL Database
    - App Services → Blob Storage
    - App Services → Redis Cache
    - App Services → Key Vault
    - Todos → Application Insights

12. **Agregar Etiquetas**
    - SKU/Tier en cada componente
    - Número de instancias
    - Tamaño/capacidad

## Plantilla para Lucidchart

### Shapes Library a Usar
- Azure Icons (si está disponible)
- Network Shapes
- Database Shapes
- Server Shapes
- Security Shapes

### Agrupación
- Agrupar componentes por ambiente
- Agrupar por tipo de servicio dentro de cada ambiente
- Usar contenedores para VNets

## Exportar el Diagrama

### Formatos Recomendados
- **PNG**: Para documentación (alta resolución, 300 DPI)
- **PDF**: Para presentaciones
- **SVG**: Para edición futura
- **Draw.io XML**: Para edición continua

## Checklist de Elementos

- [ ] 4 ambientes claramente separados
- [ ] 8 capas horizontales visibles
- [ ] Todos los componentes listados en la tabla
- [ ] Conexiones entre componentes
- [ ] Etiquetas con SKU/Tier
- [ ] Leyenda explicando símbolos
- [ ] Título y fecha del diagrama
- [ ] Notas sobre auto-scaling donde aplica
- [ ] Indicadores de alta disponibilidad en Producción

## Notas Adicionales

- Usar colores consistentes entre ambientes
- Mantener proporciones similares entre ambientes
- Agregar notas al pie explicando abreviaciones (LRS, GRS, HA, etc.)
- Incluir flechas de flujo de datos principales
- Marcar claramente qué componentes son opcionales (None) en Dev/QA

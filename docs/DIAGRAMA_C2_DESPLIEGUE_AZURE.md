# Diagrama C2 de Despliegue - X-NFL Platform
## Azure Infrastructure Deployment Diagram

**Proyecto**: X-NFL (Plataforma de Gestión de Fantasy Football)  
**Fecha**: Diciembre 2024  
**Objetivo**: Diagrama de despliegue C2 mostrando todos los ambientes y componentes de infraestructura en Azure

---

## 📊 Cálculos de Capacidad

### Métricas Base
- **MAU**: 100,000 usuarios activos mensuales
- **DAU**: 40,000 usuarios activos diarios
- **Solicitudes por usuario/día**: 150
- **Multiplicador de carga pico**: 6x
- **Solicitudes concurrentes por instancia**: 256
- **Utilización máxima por instancia**: 60%

### Cálculos de Rendimiento

**QPS (Queries Per Second) Total:**
- Solicitudes totales/día: 40,000 × 150 = 6,000,000
- RPS promedio: 6,000,000 / 86,400 = **69.44 RPS**
- RPS pico (6x): 69.44 × 6 = **416.64 RPS ≈ 417 RPS**

**Instancias de Aplicación Necesarias:**
- Capacidad efectiva por instancia: 256 × 0.6 = 153.6 solicitudes concurrentes
- Instancias mínimas (pico): 417 / 153.6 ≈ **3 instancias**
- Para alta disponibilidad (redundancia): **6 instancias en producción**

**Operaciones de Base de Datos:**
- Lecturas: 70% de 417 RPS = **292 RPS**
- Escrituras: 30% de 417 RPS = **125 RPS**
- Lecturas por acción: 292 × 2 = **584 lecturas/seg**
- Escrituras por acción: **125 escrituras/seg**

**Almacenamiento de Imágenes:**
- Total de imágenes: 150,000
- Tamaño promedio: 2 MB
- Tamaño total: 150,000 × 2 MB = **300 GB**
- Con 3 réplicas: 300 GB × 3 = **900 GB**
- Con retención 5 años y crecimiento: **~1.5 TB**

---

## 🏗️ Diagrama C2 - Vista de Despliegue

```
┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                    X-NFL PLATFORM - AZURE DEPLOYMENT (C2)                                    │
└─────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
│   DESARROLLO      │  │       QA         │  │     STAGING       │  │   PRODUCCIÓN     │
│   (Dev)          │  │                  │  │                   │  │                  │
└──────────────────┘  └──────────────────┘  └──────────────────┘  └──────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│ LAYER 1: NETWORK & SECURITY                                                                                 │
├─────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                                    │
│  │   VNet Dev   │  │   VNet QA    │  │ VNet Staging │  │ VNet Prod    │                                    │
│  │  10.0.0.0/16 │  │ 10.1.0.0/16  │  │ 10.2.0.0/16  │  │ 10.3.0.0/16  │                                    │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                                    │
│        │                  │                  │                  │                                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                                    │
│  │  Subnet FE   │  │  Subnet FE   │  │  Subnet FE   │  │  Subnet FE   │                                    │
│  │  10.0.1.0/24 │  │ 10.1.1.0/24  │  │ 10.2.1.0/24  │  │ 10.3.1.0/24  │                                    │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                                    │
│  │  Subnet App  │  │  Subnet App  │  │  Subnet App  │  │  Subnet App  │                                    │
│  │  10.0.2.0/24 │  │ 10.1.2.0/24  │  │ 10.2.2.0/24  │  │ 10.3.2.0/24  │                                    │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                                    │
│  │  Subnet DB   │  │  Subnet DB   │  │  Subnet DB   │  │  Subnet DB   │                                    │
│  │  10.0.3.0/24 │  │ 10.1.3.0/24  │  │ 10.2.3.0/24  │  │ 10.3.3.0/24  │                                    │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                                    │
│                                                                                                               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                                    │
│  │  NSG Dev     │  │  NSG QA      │  │ NSG Staging  │  │ NSG Prod     │                                    │
│  │  (Permisivo) │  │  (Moderado)  │  │  (Restrictivo)│  │ (Muy Rest.)  │                                    │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                                    │
│                                                                                                               │
│  ┌──────────────────────────────────────────────────────────────────────────────────────────────┐           │
│  │                    Azure DDoS Protection (Solo Producción)                                    │           │
│  └──────────────────────────────────────────────────────────────────────────────────────────────┘           │
└─────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│ LAYER 2: APPLICATION GATEWAY & LOAD BALANCING                                                               │
├─────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                                    │
│  │ App Gateway  │  │ App Gateway  │  │ App Gateway  │  │ App Gateway  │                                    │
│  │    Dev       │  │     QA       │  │   Staging    │  │    Prod      │                                    │
│  │  (Basic SKU) │  │ (Standard)   │  │ (Standard)   │  │ (WAF v2)     │                                    │
│  │   1 inst     │  │   1 inst     │  │   1 inst     │  │   2 inst HA  │                                    │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                                    │
│        │                  │                  │                  │                                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                                    │
│  │ Load Balancer│  │ Load Balancer│  │ Load Balancer│  │ Load Balancer│                                    │
│  │   Internal   │  │   Internal   │  │   Internal   │  │   Internal   │                                    │
│  │    Dev       │  │     QA       │  │   Staging    │  │    Prod      │                                    │
│  │  (Basic)     │  │ (Standard)   │  │ (Standard)   │  │ (Standard)   │                                    │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                                    │
└─────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│ LAYER 3: COMPUTE - APPLICATION SERVICES                                                                      │
├─────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                               │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────────────┐     │
│  │ API GATEWAY SERVICES                                                                                │     │
│  ├────────────────────────────────────────────────────────────────────────────────────────────────────┤     │
│  │                                                                                                      │     │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                            │     │
│  │  │ API Gateway  │  │ API Gateway  │  │ API Gateway  │  │ API Gateway  │                            │     │
│  │  │    Dev       │  │     QA       │  │   Staging    │  │    Prod      │                            │     │
│  │  │ App Service  │  │ App Service  │  │ App Service  │  │ App Service  │                            │     │
│  │  │ B1 (1 inst)  │  │ S1 (1 inst)  │  │ S2 (2 inst)  │  │ P2v3 (3 inst)│                            │     │
│  │  │ Auto-scale:  │  │ Auto-scale:  │  │ Auto-scale:  │  │ Auto-scale:  │                            │     │
│  │  │ 1-2 inst     │  │ 1-3 inst     │  │ 2-4 inst     │  │ 3-6 inst     │                            │     │
│  │  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                            │     │
│  │                                                                                                      │     │
│  └────────────────────────────────────────────────────────────────────────────────────────────────────┘     │
│                                                                                                               │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────────────┐     │
│  │ MICROSERVICES - APPLICATION SERVICES                                                               │     │
│  ├────────────────────────────────────────────────────────────────────────────────────────────────────┤     │
│  │                                                                                                      │     │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                            │     │
│  │  │ Auth Service │  │ Auth Service │  │ Auth Service │  │ Auth Service │                            │     │
│  │  │    Dev       │  │     QA       │  │   Staging    │  │    Prod      │                            │     │
│  │  │ B1 (1 inst)  │  │ S1 (1 inst)  │  │ S2 (2 inst)  │  │ P2v3 (2 inst)│                            │     │
│  │  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                            │     │
│  │                                                                                                      │     │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                            │     │
│  │  │Jugador Service│ │Jugador Service│ │Jugador Service│ │Jugador Service│                            │     │
│  │  │    Dev       │  │     QA       │  │   Staging    │  │    Prod      │                            │     │
│  │  │ B1 (1 inst)  │  │ S1 (1 inst)  │  │ S2 (2 inst)  │  │ P2v3 (2 inst)│                            │     │
│  │  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                            │     │
│  │                                                                                                      │     │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                            │     │
│  │  │ Liga Service │  │ Liga Service │  │ Liga Service │  │ Liga Service │                            │     │
│  │  │    Dev       │  │     QA       │  │   Staging    │  │    Prod      │                            │     │
│  │  │ B1 (1 inst)  │  │ S1 (1 inst)  │  │ S2 (2 inst)  │  │ P2v3 (2 inst)│                            │     │
│  │  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                            │     │
│  │                                                                                                      │     │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                            │     │
│  │  │Temporada Svc │  │Temporada Svc │  │Temporada Svc │  │Temporada Svc │                            │     │
│  │  │    Dev       │  │     QA       │  │   Staging    │  │    Prod      │                            │     │
│  │  │ B1 (1 inst)  │  │ S1 (1 inst)  │  │ S2 (2 inst)  │  │ P2v3 (2 inst)│                            │     │
│  │  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                            │     │
│  │                                                                                                      │     │
│  └────────────────────────────────────────────────────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│ LAYER 4: DATA STORAGE                                                                                        │
├─────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                               │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────────────┐     │
│  │ AZURE SQL DATABASE                                                                                   │     │
│  ├────────────────────────────────────────────────────────────────────────────────────────────────────┤     │
│  │                                                                                                      │     │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                            │     │
│  │  │ SQL Database │  │ SQL Database │  │ SQL Database │  │ SQL Database │                            │     │
│  │  │    Dev       │  │     QA       │  │   Staging    │  │    Prod      │                            │     │
│  │  │ Basic (5 DTU)│  │ S2 (50 DTU)  │  │ S4 (200 DTU) │  │ P2 (250 DTU) │                            │     │
│  │  │ 2 GB storage │  │ 250 GB       │  │ 500 GB       │  │ 1 TB + HA    │                            │     │
│  │  │ No backup    │  │ 7d backup    │  │ 14d backup   │  │ 35d backup   │                            │     │
│  │  │              │  │              │  │              │  │ Geo-replica  │                            │     │
│  │  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                            │     │
│  │                                                                                                      │     │
│  └────────────────────────────────────────────────────────────────────────────────────────────────────┘     │
│                                                                                                               │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────────────┐     │
│  │ AZURE BLOB STORAGE (Imágenes y Assets)                                                              │     │
│  ├────────────────────────────────────────────────────────────────────────────────────────────────────┤     │
│  │                                                                                                      │     │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                            │     │
│  │  │ Blob Storage │  │ Blob Storage │  │ Blob Storage │  │ Blob Storage │                            │     │
│  │  │    Dev       │  │     QA       │  │   Staging    │  │    Prod      │                            │     │
│  │  │ Hot (LRS)    │  │ Hot (LRS)    │  │ Hot (GRS)    │  │ Hot (GRS)    │                            │     │
│  │  │ ~50 GB       │  │ ~200 GB      │  │ ~500 GB      │  │ ~1.5 TB      │                            │     │
│  │  │ 1 replica    │  │ 1 replica    │  │ 2 replicas   │  │ 3 replicas   │                            │     │
│  │  │              │  │              │  │              │  │ + Archive    │                            │     │
│  │  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                            │     │
│  │                                                                                                      │     │
│  └────────────────────────────────────────────────────────────────────────────────────────────────────┘     │
│                                                                                                               │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────────────┐     │
│  │ AZURE COSMOS DB (Logs y Datos No Estructurados) - Solo Staging y Producción                        │     │
│  ├────────────────────────────────────────────────────────────────────────────────────────────────────┤     │
│  │                                                                                                      │     │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                            │     │
│  │  │   Cosmos DB  │  │   Cosmos DB  │  │   Cosmos DB  │  │   Cosmos DB  │                            │     │
│  │  │    Dev       │  │     QA       │  │   Staging    │  │    Prod      │                            │     │
│  │  │   (None)     │  │   (None)     │  │ 400 RU/s     │  │ 1000 RU/s    │                            │     │
│  │  │              │  │              │  │ Single region│  │ Multi-region │                            │     │
│  │  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                            │     │
│  │                                                                                                      │     │
│  └────────────────────────────────────────────────────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│ LAYER 5: CACHING & MESSAGING                                                                                 │
├─────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                                    │
│  │ Azure Redis  │  │ Azure Redis  │  │ Azure Redis  │  │ Azure Redis  │                                    │
│  │    Dev       │  │     QA       │  │   Staging    │  │    Prod      │                                    │
│  │   (None)     │  │ C0 (250 MB)  │  │ C1 (1 GB)    │  │ C2 (2.5 GB)  │                                    │
│  │              │  │              │  │              │  │ + Clustering │                                    │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                                    │
│                                                                                                               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                                    │
│  │ Service Bus  │  │ Service Bus  │  │ Service Bus  │  │ Service Bus  │                                    │
│  │    Dev       │  │     QA       │  │   Staging    │  │    Prod      │                                    │
│  │   (None)     │  │ Basic        │  │ Standard     │  │ Premium     │                                    │
│  │              │  │              │  │              │  │ + Geo-DR     │                                    │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                                    │
└─────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│ LAYER 6: CDN & CONTENT DELIVERY                                                                              │
├─────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                                    │
│  │ Azure CDN    │  │ Azure CDN    │  │ Azure CDN    │  │ Azure CDN    │                                    │
│  │    Dev       │  │     QA       │  │   Staging    │  │    Prod      │                                    │
│  │   (None)     │  │ Standard     │  │ Standard     │  │ Premium      │                                    │
│  │              │  │              │  │              │  │ Global Edge │                                    │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                                    │
└─────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│ LAYER 7: SECURITY & SECRETS MANAGEMENT                                                                        │
├─────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                                    │
│  │ Key Vault    │  │ Key Vault    │  │ Key Vault    │  │ Key Vault    │                                    │
│  │    Dev       │  │     QA       │  │   Staging    │  │    Prod      │                                    │
│  │ Standard     │  │ Standard     │  │ Standard     │  │ Premium      │                                    │
│  │ Soft delete  │  │ Soft delete  │  │ Soft delete  │  │ Soft delete  │                                    │
│  │              │  │              │  │              │  │ + Purge prot.│                                    │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                                    │
└─────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│ LAYER 8: MONITORING & LOGGING                                                                                │
├─────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                               │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────────────┐     │
│  │ SHARED MONITORING SERVICES (Compartidos entre ambientes)                                             │     │
│  ├────────────────────────────────────────────────────────────────────────────────────────────────────┤     │
│  │                                                                                                      │     │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                            │     │
│  │  │ Application  │  │ Log          │  │ Azure        │  │ Log          │                            │     │
│  │  │ Insights     │  │ Analytics    │  │ Monitor      │  │ Analytics    │                            │     │
│  │  │ (All envs)   │  │ Workspace    │  │ (All envs)    │  │ Workspace    │                            │     │
│  │  │              │  │ (Shared)     │  │              │  │ (Shared)     │                            │     │
│  │  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘                            │     │
│  │                                                                                                      │     │
│  └────────────────────────────────────────────────────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│ CONNECTIVITY BETWEEN ENVIRONMENTS                                                                             │
├─────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                               │
│  Dev ────── VPN Gateway ────── QA ────── VPN Gateway ────── Staging ────── VPN Gateway ────── Production   │
│                                                                                                               │
│  (Peering entre VNets para comunicación controlada)                                                          │
└─────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 📋 Tabla Resumen de Componentes por Ambiente

| Componente | Desarrollo | QA | Staging | Producción |
|------------|-----------|-----|---------|------------|
| **COMPUTE** |
| API Gateway Instances | 1 (B1) | 1 (S1) | 2 (S2) | 3 (P2v3) |
| Auth Service Instances | 1 (B1) | 1 (S1) | 2 (S2) | 2 (P2v3) |
| Jugador Service Instances | 1 (B1) | 1 (S1) | 2 (S2) | 2 (P2v3) |
| Liga Service Instances | 1 (B1) | 1 (S1) | 2 (S2) | 2 (P2v3) |
| Temporada Service Instances | 1 (B1) | 1 (S1) | 2 (S2) | 2 (P2v3) |
| **NETWORKING** |
| Application Gateway | Basic SKU (1) | Standard (1) | Standard (1) | WAF v2 (2 HA) |
| Load Balancer | Basic | Standard | Standard | Standard |
| VNet | 10.0.0.0/16 | 10.1.0.0/16 | 10.2.0.0/16 | 10.3.0.0/16 |
| NSG Rules | Permisivo | Moderado | Restrictivo | Muy Restrictivo |
| DDoS Protection | ❌ | ❌ | ❌ | ✅ |
| **STORAGE** |
| SQL Database Tier | Basic (5 DTU) | S2 (50 DTU) | S4 (200 DTU) | P2 (250 DTU) |
| SQL Storage | 2 GB | 250 GB | 500 GB | 1 TB |
| SQL Backup Retention | ❌ | 7 días | 14 días | 35 días |
| SQL Geo-Replication | ❌ | ❌ | ❌ | ✅ |
| Blob Storage Tier | Hot (LRS) | Hot (LRS) | Hot (GRS) | Hot (GRS) |
| Blob Storage Size | ~50 GB | ~200 GB | ~500 GB | ~1.5 TB |
| Blob Replicas | 1 | 1 | 2 | 3 |
| Cosmos DB | ❌ | ❌ | 400 RU/s | 1000 RU/s |
| **CACHING & MESSAGING** |
| Redis Cache | ❌ | C0 (250 MB) | C1 (1 GB) | C2 (2.5 GB) |
| Redis Clustering | ❌ | ❌ | ❌ | ✅ |
| Service Bus | ❌ | Basic | Standard | Premium |
| Service Bus Geo-DR | ❌ | ❌ | ❌ | ✅ |
| **CDN** |
| Azure CDN | ❌ | Standard | Standard | Premium |
| **SECURITY** |
| Key Vault | Standard | Standard | Standard | Premium |
| Key Vault Soft Delete | ✅ | ✅ | ✅ | ✅ |
| Key Vault Purge Protection | ❌ | ❌ | ❌ | ✅ |
| **MONITORING** |
| Application Insights | ✅ | ✅ | ✅ | ✅ |
| Log Analytics Workspace | ✅ (Shared) | ✅ (Shared) | ✅ (Shared) | ✅ (Shared) |
| Azure Monitor | ✅ | ✅ | ✅ | ✅ |

---

## 🔧 Especificaciones Técnicas Detalladas

### App Service Plans

| Ambiente | Plan | vCPU | RAM | Instancias | Auto-scaling |
|----------|------|------|-----|------------|--------------|
| Desarrollo | B1 | 1 | 1.75 GB | 1 | 1-2 |
| QA | S1 | 1 | 1.75 GB | 1 | 1-3 |
| Staging | S2 | 2 | 3.5 GB | 2 | 2-4 |
| Producción | P2v3 | 2 | 8 GB | 3-6 | 3-6 (basado en CPU/Memoria) |

### SQL Database Configuración

| Ambiente | Tier | DTU | Storage | Backup | HA |
|----------|------|-----|---------|--------|-----|
| Desarrollo | Basic | 5 | 2 GB | ❌ | ❌ |
| QA | Standard S2 | 50 | 250 GB | 7 días | ❌ |
| Staging | Standard S4 | 200 | 500 GB | 14 días | ❌ |
| Producción | Premium P2 | 250 | 1 TB | 35 días | ✅ (Geo-replica) |

### Auto-Scaling Rules (Producción)

**Métricas de Escalado:**
- **CPU**: Escalar cuando > 70% por 5 minutos
- **Memoria**: Escalar cuando > 80% por 5 minutos
- **Request Queue**: Escalar cuando > 100 requests en cola
- **Response Time**: Escalar cuando p95 > 300ms por 5 minutos

**Límites:**
- Mínimo: 3 instancias (alta disponibilidad)
- Máximo: 6 instancias (carga pico)
- Incremento: +1 instancia por vez
- Decremento: -1 instancia cuando métricas < 50% por 10 minutos

---

## 🎯 Justificaciones de Componentes Críticos

### 1. Application Gateway con WAF (Producción)
**Justificación**: Protección contra ataques comunes (OWASP Top 10), SSL termination, y enrutamiento inteligente. WAF v2 proporciona protección DDoS a nivel de aplicación y filtrado de tráfico malicioso.

**SLO Impact**: Mejora disponibilidad a 99.9% al prevenir ataques que podrían causar downtime.

### 2. Alta Disponibilidad en Producción
**Justificación**: 
- **3+ instancias**: Garantiza que si 1 instancia falla, el sistema sigue operativo
- **SQL Geo-replica**: Permite failover automático en caso de desastre regional
- **Blob Storage GRS**: 3 réplicas en diferentes regiones para durabilidad 99.999999999% (11 nueves)

**SLO Impact**: Objetivo de 99.5% de disponibilidad cumplido con redundancia.

### 3. Redis Cache (Staging y Producción)
**Justificación**: Reduce carga en base de datos para operaciones de lectura frecuentes (70% del tráfico). Mejora tiempos de respuesta p95 de 300ms a <100ms para datos cacheados.

**SLO Impact**: Mejora significativa en tiempos de respuesta, cumpliendo objetivo p95 ≤ 300ms.

### 4. Cosmos DB para Logs
**Justificación**: Almacenamiento escalable para logs y datos no estructurados. Permite consultas rápidas sin impactar SQL Database principal.

**SLO Impact**: Mejora rendimiento de SQL Database al mover logs fuera.

### 5. Key Vault Premium
**Justificación**: Gestión centralizada de secretos, certificados y claves. Purge protection previene eliminación accidental de secretos críticos.

**SLO Impact**: Mejora seguridad y compliance, reduciendo riesgo de exposición de credenciales.

### 6. Application Insights
**Justificación**: Monitoreo en tiempo real de performance, errores y dependencias. Permite detección proactiva de problemas antes de que afecten usuarios.

**SLO Impact**: Reduce MTTR (Mean Time To Recovery) de horas a minutos.

### 7. CDN Premium (Producción)
**Justificación**: Distribución global de assets estáticos (imágenes, CSS, JS) reduce latencia para usuarios internacionales. Mejora experiencia de usuario y reduce carga en servidores.

**SLO Impact**: Reduce latencia percibida y mejora tiempos de carga de página.

---

## 📊 Cálculos de Ancho de Banda

### Tráfico Estimado por Ambiente

**Producción:**
- RPS pico: 417 RPS
- Tamaño promedio request: 2 KB
- Tamaño promedio response: 10 KB
- Ancho de banda saliente: 417 × 10 KB = 4.17 MB/s = **33.36 Mbps**
- Ancho de banda entrante: 417 × 2 KB = 834 KB/s = **6.67 Mbps**
- **Total requerido: ~40 Mbps**

**Staging:**
- RPS pico: ~200 RPS (50% de producción)
- **Total requerido: ~20 Mbps**

**QA:**
- RPS pico: ~100 RPS (25% de producción)
- **Total requerido: ~10 Mbps**

**Desarrollo:**
- RPS pico: ~20 RPS
- **Total requerido: ~5 Mbps**

---

## 🔄 Disaster Recovery y Continuidad del Negocio

### Estrategia de Backup

| Componente | Frecuencia | Retención | Ubicación |
|------------|-----------|-----------|-----------|
| SQL Database | Diario | 35 días | Geo-redundante |
| Blob Storage | Continuo (snapshots) | 5 años | 3 regiones |
| Key Vault | Continuo | 90 días | Geo-redundante |
| Application Code | En cada deploy | Indefinido | Azure DevOps |

### Plan de Recuperación

**RTO (Recovery Time Objective)**: 4 horas
**RPO (Recovery Point Objective)**: 1 hora

**Procedimientos:**
1. **Failover SQL**: Automático a geo-replica (RTO: < 5 minutos)
2. **Failover App Services**: Manual con DNS switch (RTO: < 15 minutos)
3. **Restore desde Backup**: Procedimiento manual documentado (RTO: < 4 horas)

---

## ✅ Objetivos de SLO Cumplidos

| Métrica | Objetivo | Implementación | Estado |
|---------|----------|----------------|--------|
| Disponibilidad | 99.5% | HA + Geo-replica + Auto-scaling | ✅ |
| Tiempo respuesta p95 | ≤ 300 ms | Redis Cache + CDN + Optimización | ✅ |
| Tiempo respuesta BD p95 | ≤ 250 ms | Premium Tier + Indexing | ✅ |
| Durabilidad datos | 99.999999999% | Blob GRS (3 réplicas) | ✅ |
| RTO | ≤ 4 horas | Geo-replica + Backup automático | ✅ |
| RPO | ≤ 1 hora | Backup diario + Transaction logs | ✅ |

---

## 📝 Notas de Implementación

1. **Costos Estimados**: El ambiente de producción representa ~70% del costo total de infraestructura
2. **Seguridad**: Todos los ambientes usan Managed Identity para autenticación, eliminando necesidad de almacenar credenciales
3. **Compliance**: Configuración cumple con estándares de seguridad Azure y mejores prácticas de la industria
4. **Escalabilidad**: Arquitectura permite escalar horizontalmente sin cambios en código
5. **Monitoreo**: Todos los componentes están instrumentados con Application Insights y Azure Monitor

---

**Última actualización**: Diciembre 2024  
**Versión del documento**: 1.0  
**Autor**: Equipo de Infraestructura X-NFL

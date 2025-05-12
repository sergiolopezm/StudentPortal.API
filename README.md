# **ESTRUCTURA DE SOFTWARE**

# **SERVICIO STUDENTPORTAL API**

|  |  |
| --- | --- |
| **CAPA** | BACKEND |
| **PLATAFORMA** | SERVER – WINDOWS |
| **TIPO** | .NET |

## 1. DESCRIPCIÓN GENERAL

El servicio StudentPortal API proporciona una interfaz para la gestión de un portal académico, permitiendo la administración de estudiantes, profesores, materias, programas académicos e inscripciones. El sistema incluye funcionalidades de autenticación basada en JWT, gestión de usuarios con diferentes roles y registro de actividades.

La API está diseñada siguiendo principios RESTful y utiliza Entity Framework Core para la comunicación con la base de datos SQL Server. Proporciona endpoints para realizar operaciones CRUD (Crear, Leer, Actualizar, Eliminar) sobre las entidades académicas y gestión de usuarios.

## 2. REQUISITOS PREVIOS

### 2.1. Estructura de Base de Datos

Para el funcionamiento correcto del sistema, es necesario crear las siguientes tablas en la base de datos:

#### 2.1.1. Tabla Accesos
```sql
CREATE TABLE Accesos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Sitio NVARCHAR(50) NOT NULL,
    Contraseña NVARCHAR(250) NOT NULL,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaModificacion DATETIME2 NULL,
    Activo BIT NOT NULL DEFAULT 1
);
```

#### 2.1.2. Tabla Roles
```sql
CREATE TABLE Roles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    Descripcion NVARCHAR(200) NULL,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaModificacion DATETIME2 NULL,
    Activo BIT NOT NULL DEFAULT 1
);
```

#### 2.1.3. Tabla Usuarios
```sql
CREATE TABLE Usuarios (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    NombreUsuario NVARCHAR(100) NOT NULL UNIQUE,
    Contraseña NVARCHAR(250) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Apellido NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    RolId INT NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaModificacion DATETIME2 NULL,
    FechaUltimoAcceso DATETIME2 NULL,
    FOREIGN KEY (RolId) REFERENCES Roles(Id)
);
```

#### 2.1.4. Tabla Tokens
```sql
CREATE TABLE Tokens (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Token NVARCHAR(1000) NOT NULL,
    UsuarioId UNIQUEIDENTIFIER NOT NULL,
    Ip NVARCHAR(45) NOT NULL,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaExpiracion DATETIME2 NOT NULL,
    Observacion NVARCHAR(200) NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);
```

#### 2.1.5. Tabla TokensExpirados
```sql
CREATE TABLE TokensExpirados (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Token NVARCHAR(1000) NOT NULL,
    UsuarioId UNIQUEIDENTIFIER NOT NULL,
    Ip NVARCHAR(45) NOT NULL,
    FechaCreacion DATETIME2 NOT NULL,
    FechaExpiracion DATETIME2 NOT NULL,
    Observacion NVARCHAR(200) NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);
```

#### 2.1.6. Tabla Logs
```sql
CREATE TABLE Logs (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Fecha DATETIME2 NOT NULL DEFAULT GETDATE(),
    Tipo NVARCHAR(50) NOT NULL,
    UsuarioId UNIQUEIDENTIFIER NULL,
    Ip NVARCHAR(45) NULL,
    Accion NVARCHAR(200) NULL,
    Detalle NVARCHAR(MAX) NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);
```

#### 2.1.7. Tabla Programas
```sql
CREATE TABLE Programas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    CreditosMinimos INT NOT NULL DEFAULT 0,
    CreditosMaximos INT NOT NULL DEFAULT 9,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaModificacion DATETIME2 NULL,
    CreadoPorId UNIQUEIDENTIFIER NULL,
    ModificadoPorId UNIQUEIDENTIFIER NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (CreadoPorId) REFERENCES Usuarios(Id),
    FOREIGN KEY (ModificadoPorId) REFERENCES Usuarios(Id)
);
```

#### 2.1.8. Tabla Materias
```sql
CREATE TABLE Materias (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Codigo NVARCHAR(20) NOT NULL UNIQUE,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    Creditos INT NOT NULL DEFAULT 3,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaModificacion DATETIME2 NULL,
    CreadoPorId UNIQUEIDENTIFIER NULL,
    ModificadoPorId UNIQUEIDENTIFIER NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (CreadoPorId) REFERENCES Usuarios(Id),
    FOREIGN KEY (ModificadoPorId) REFERENCES Usuarios(Id)
);
```

#### 2.1.9. Tabla Profesores
```sql
CREATE TABLE Profesores (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId UNIQUEIDENTIFIER NOT NULL UNIQUE,
    Identificacion NVARCHAR(50) NOT NULL UNIQUE,
    Telefono NVARCHAR(20) NULL,
    Departamento NVARCHAR(100) NULL,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaModificacion DATETIME2 NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);
```

#### 2.1.10. Tabla ProfesorMaterias
```sql
CREATE TABLE ProfesorMaterias (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProfesorId INT NOT NULL,
    MateriaId INT NOT NULL,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    Activo BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (ProfesorId) REFERENCES Profesores(Id),
    FOREIGN KEY (MateriaId) REFERENCES Materias(Id),
    UNIQUE(ProfesorId, MateriaId)
);
```

#### 2.1.11. Tabla Estudiantes
```sql
CREATE TABLE Estudiantes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId UNIQUEIDENTIFIER NOT NULL UNIQUE,
    Identificacion NVARCHAR(50) NOT NULL UNIQUE,
    Telefono NVARCHAR(20) NULL,
    Carrera NVARCHAR(100) NULL,
    ProgramaId INT NOT NULL,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaModificacion DATETIME2 NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    FOREIGN KEY (ProgramaId) REFERENCES Programas(Id)
);
```

#### 2.1.12. Tabla InscripcionesEstudiantes
```sql
CREATE TABLE InscripcionesEstudiantes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EstudianteId INT NOT NULL,
    MateriaId INT NOT NULL,
    FechaInscripcion DATETIME2 NOT NULL DEFAULT GETDATE(),
    Estado NVARCHAR(50) NOT NULL DEFAULT 'Inscrito',
    Calificacion DECIMAL(3,2) NULL,
    FechaModificacion DATETIME2 NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (EstudianteId) REFERENCES Estudiantes(Id),
    FOREIGN KEY (MateriaId) REFERENCES Materias(Id),
    UNIQUE(EstudianteId, MateriaId)
);
```

#### 2.1.13. Vista CompañerosClase
```sql
CREATE VIEW VW_CompanerosClase AS
SELECT DISTINCT 
    ie1.EstudianteId AS EstudianteId,
    ie1.MateriaId,
    m.Nombre AS MateriaNombre,
    ie2.EstudianteId AS CompaneroId,
    u.Nombre + ' ' + u.Apellido AS CompaneroNombre
FROM InscripcionesEstudiantes ie1
INNER JOIN InscripcionesEstudiantes ie2 ON ie1.MateriaId = ie2.MateriaId AND ie1.EstudianteId != ie2.EstudianteId
INNER JOIN Materias m ON ie1.MateriaId = m.Id
INNER JOIN Estudiantes e ON ie2.EstudianteId = e.Id
INNER JOIN Usuarios u ON e.UsuarioId = u.Id
WHERE ie1.Estado = 'Inscrito' AND ie2.Estado = 'Inscrito'
    AND ie1.Activo = 1 AND ie2.Activo = 1;
```

#### 2.1.14. Triggers

##### 2.1.14.1. Trigger ValidarMaximoMaterias
```sql
CREATE TRIGGER TR_ValidarMaximoMaterias
ON InscripcionesEstudiantes
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (
        SELECT 1
        FROM inserted i
        WHERE (
            SELECT COUNT(*)
            FROM InscripcionesEstudiantes ie
            WHERE ie.EstudianteId = i.EstudianteId 
                AND ie.Estado = 'Inscrito'
                AND ie.Activo = 1
        ) > 3
    )
    BEGIN
        RAISERROR ('Un estudiante no puede inscribir más de 3 materias', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
```

##### 2.1.14.2. Trigger ValidarProfesorDiferente
```sql
CREATE TRIGGER TR_ValidarProfesorDiferente
ON InscripcionesEstudiantes
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (
        SELECT i.EstudianteId, pm.ProfesorId
        FROM inserted i
        INNER JOIN ProfesorMaterias pm ON i.MateriaId = pm.MateriaId
        WHERE pm.Activo = 1 AND i.Activo = 1
        GROUP BY i.EstudianteId, pm.ProfesorId
        HAVING COUNT(*) > 1
    )
    BEGIN
        RAISERROR ('Un estudiante no puede tener clases con el mismo profesor en diferentes materias', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
```

### 2.2. Datos Iniciales

Es necesario insertar los siguientes registros iniciales:

```sql
-- Insertar roles iniciales
INSERT INTO Roles (Nombre, Descripcion) VALUES 
('Admin', 'Administrador del sistema'),
('Profesor', 'Profesor que dicta materias'),
('Estudiante', 'Estudiante inscrito en el programa');

-- Insertar administrador inicial
-- Contraseña: AdminStudent2025* (en SHA256)
INSERT INTO Usuarios (NombreUsuario, Contraseña, Nombre, Apellido, Email, RolId)
VALUES ('admin', 'D91B14B22BB2BE1CFB05212DC262646F17220C0860A9886ACD7CF94D7CFE4F43', 'Administrador', 'Sistema', 'admin@sistema.com', 1);

-- Insertar configuración de acceso
INSERT INTO Accesos (Sitio, Contraseña) VALUES ('StudentPortal', 'Portal123');

-- Insertar programa inicial
INSERT INTO Programas (Nombre, Descripcion, CreditosMinimos, CreditosMaximos)
VALUES ('Programa General', 'Programa general de créditos para estudiantes', 9, 9);

-- Insertar materias iniciales
INSERT INTO Materias (Codigo, Nombre, Descripcion, Creditos) VALUES 
('MAT101', 'Matemáticas I', 'Fundamentos de matemáticas', 3),
('MAT102', 'Cálculo I', 'Introducción al cálculo diferencial', 3),
('FIS101', 'Física I', 'Mecánica clásica', 3),
('QUI101', 'Química General', 'Principios de química', 3),
('PRG101', 'Programación I', 'Introducción a la programación', 3),
('PRG102', 'Programación II', 'Programación orientada a objetos', 3),
('BIO101', 'Biología General', 'Fundamentos de biología', 3),
('EST101', 'Estadística I', 'Introducción a la estadística', 3),
('COM101', 'Comunicación', 'Técnicas de comunicación', 3),
('ING101', 'Inglés I', 'Inglés básico', 3);
```

## 3. MÉTODOS

### 3.1. Autenticación

#### 3.1.1. Login

Autentica un usuario en el sistema y devuelve un token JWT.

Acceso: `api/Auth/login`  
Formato: JSON  
Servicio: REST / POST  
Autenticación: Requiere headers de acceso (Sitio, Clave)

##### 3.1.1.1. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| nombreUsuario | Nombre de usuario | String | Sí |
| contraseña | Contraseña del usuario | String | Sí |
| ip | Dirección IP del cliente | String | No |

Ejemplo de entrada:
```json
{
  "nombreUsuario": "admin",
  "contraseña": "Admin123",
  "ip": "192.168.1.1"
}
```

##### 3.1.1.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto con datos del usuario autenticado | Object |
| resultado.usuario | Datos del usuario | Object |
| resultado.usuario.id | Identificador único del usuario | GUID |
| resultado.usuario.nombreUsuario | Nombre de usuario | String |
| resultado.usuario.nombre | Nombre real del usuario | String |
| resultado.usuario.apellido | Apellido del usuario | String |
| resultado.usuario.email | Correo electrónico del usuario | String |
| resultado.usuario.rol | Rol del usuario en el sistema | String |
| resultado.usuario.rolId | ID del rol del usuario | Integer |
| resultado.usuario.activo | Estado de activación del usuario | Boolean |
| resultado.usuario.fechaCreacion | Fecha de creación del usuario | DateTime |
| resultado.usuario.fechaUltimoAcceso | Fecha del último acceso | DateTime |
| resultado.token | Token JWT para autenticación | String |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Usuario autenticado",
  "detalle": "El usuario admin se ha autenticado correctamente.",
  "resultado": {
    "usuario": {
      "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "nombreUsuario": "admin",
      "nombre": "Administrador",
      "apellido": "Sistema",
      "email": "admin@sistema.com",
      "rol": "Admin",
      "rolId": 1,
      "activo": true,
      "fechaCreacion": "2023-01-01T00:00:00",
      "fechaUltimoAcceso": "2024-05-12T12:30:45"
    },
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

#### 3.1.2. Registro

Registra un nuevo usuario en el sistema.

Acceso: `api/Auth/registro`  
Formato: JSON  
Servicio: REST / POST  
Autenticación: Requiere headers de acceso (Sitio, Clave) y JWT requerido

##### 3.1.2.1. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| nombreUsuario | Nombre de usuario | String | Sí |
| contraseña | Contraseña del usuario | String | Sí |
| nombre | Nombre real del usuario | String | Sí |
| apellido | Apellido del usuario | String | Sí |
| email | Correo electrónico | String | Sí |
| rolId | ID del rol asignado | Integer | Sí |

Ejemplo de entrada:
```json
{
  "nombreUsuario": "usuario1",
  "contraseña": "Password123",
  "nombre": "Juan",
  "apellido": "Pérez",
  "email": "juan.perez@ejemplo.com",
  "rolId": 3
}
```

##### 3.1.2.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto con datos del usuario registrado | Object |
| resultado.id | Identificador único del usuario | GUID |
| resultado.nombreUsuario | Nombre de usuario | String |
| resultado.email | Correo electrónico del usuario | String |
| resultado.rol | Rol del usuario en el sistema | String |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Usuario registrado",
  "detalle": "El usuario se ha registrado correctamente",
  "resultado": {
    "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "nombreUsuario": "usuario1",
    "email": "juan.perez@ejemplo.com",
    "rol": "Estudiante"
  }
}
```

#### 3.1.3. Registro Completo

Registra un nuevo usuario en el sistema con información adicional según su rol (estudiante o profesor).

Acceso: `api/Auth/registroCompleto`  
Formato: JSON  
Servicio: REST / POST  
Autenticación: Requiere headers de acceso (Sitio, Clave) y JWT requerido

##### 3.1.3.1. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| nombreUsuario | Nombre de usuario | String | Sí |
| contraseña | Contraseña del usuario | String | Sí |
| nombre | Nombre real del usuario | String | Sí |
| apellido | Apellido del usuario | String | Sí |
| email | Correo electrónico | String | Sí |
| rolId | ID del rol asignado | Integer | Sí |
| estudianteInfo | Información de estudiante (si rolId=3) | Object | No |
| estudianteInfo.identificacion | Identificación del estudiante | String | Sí (si rolId=3) |
| estudianteInfo.telefono | Teléfono del estudiante | String | No |
| estudianteInfo.carrera | Carrera del estudiante | String | No |
| estudianteInfo.programaId | ID del programa académico | Integer | Sí (si rolId=3) |
| profesorInfo | Información de profesor (si rolId=2) | Object | No |
| profesorInfo.identificacion | Identificación del profesor | String | Sí (si rolId=2) |
| profesorInfo.telefono | Teléfono del profesor | String | No |
| profesorInfo.departamento | Departamento del profesor | String | No |

Ejemplo de entrada (estudiante):
```json
{
  "nombreUsuario": "estudiante1",
  "contraseña": "Password123",
  "nombre": "Ana",
  "apellido": "Gómez",
  "email": "ana.gomez@ejemplo.com",
  "rolId": 3,
  "estudianteInfo": {
    "identificacion": "1098765432",
    "telefono": "555-4321",
    "carrera": "Ingeniería de Sistemas",
    "programaId": 1
  }
}
```

##### 3.1.3.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto con datos del usuario registrado | Object |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Estudiante creado",
  "detalle": "El estudiante 'Ana Gómez' ha sido creado correctamente",
  "resultado": {
    "id": 1,
    "usuarioId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "identificacion": "1098765432",
    "telefono": "555-4321",
    "carrera": "Ingeniería de Sistemas",
    "programaId": 1,
    "activo": true,
    "fechaCreacion": "2024-05-12T14:30:00",
    "nombreCompleto": "Ana Gómez",
    "email": "ana.gomez@ejemplo.com",
    "programa": "Programa General",
    "materiasInscritasCount": 0,
    "creditosActuales": 0
  }
}
```

#### 3.1.4. Obtener Perfil

Obtiene el perfil del usuario autenticado.

Acceso: `api/Auth/perfil`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.1.4.1. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto con datos del perfil | Object |
| resultado.id | Identificador único del usuario | GUID |
| resultado.nombreUsuario | Nombre de usuario | String |
| resultado.nombre | Nombre real del usuario | String |
| resultado.apellido | Apellido del usuario | String |
| resultado.email | Correo electrónico del usuario | String |
| resultado.rol | Rol del usuario en el sistema | String |
| resultado.rolId | ID del rol del usuario | Integer |
| resultado.activo | Estado de activación del usuario | Boolean |
| resultado.fechaCreacion | Fecha de creación del usuario | DateTime |
| resultado.fechaUltimoAcceso | Fecha del último acceso | DateTime |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Perfil obtenido",
  "detalle": "Perfil de usuario obtenido correctamente",
  "resultado": {
    "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "nombreUsuario": "admin",
    "nombre": "Administrador",
    "apellido": "Sistema",
    "email": "admin@sistema.com",
    "rol": "Admin",
    "rolId": 1,
    "activo": true,
    "fechaCreacion": "2023-01-01T00:00:00",
    "fechaUltimoAcceso": "2024-05-12T12:30:45"
  }
}
```

#### 3.1.5. Logout

Cierra la sesión del usuario invalidando el token actual.

Acceso: `api/Auth/logout`  
Formato: JSON  
Servicio: REST / POST  
Autenticación: JWT requerido

##### 3.1.5.1. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto de resultado (nulo en este caso) | Object |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Logout exitoso",
  "detalle": "Sesión cerrada correctamente",
  "resultado": null
}
```

### 3.2. Gestión de Estudiantes

#### 3.2.1. Obtener Todos los Estudiantes

Obtiene la lista de todos los estudiantes activos.

Acceso: `api/Estudiante`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.2.1.1. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Lista de estudiantes | Array |
| resultado[].id | Identificador único del estudiante | Integer |
| resultado[].usuarioId | ID del usuario asociado | GUID |
| resultado[].identificacion | Número de identificación | String |
| resultado[].telefono | Número telefónico | String |
| resultado[].carrera | Nombre de la carrera | String |
| resultado[].programaId | ID del programa asociado | Integer |
| resultado[].activo | Estado de activación | Boolean |
| resultado[].fechaCreacion | Fecha de creación | DateTime |
| resultado[].fechaModificacion | Fecha de última modificación | DateTime |
| resultado[].nombreCompleto | Nombre completo del estudiante | String |
| resultado[].email | Correo electrónico | String |
| resultado[].programa | Nombre del programa | String |
| resultado[].materiasInscritasCount | Cantidad de materias inscritas | Integer |
| resultado[].creditosActuales | Total de créditos actuales | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Estudiantes obtenidos",
  "detalle": "Se han obtenido 2 estudiantes",
  "resultado": [
    {
      "id": 1,
      "usuarioId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "identificacion": "1098765432",
      "telefono": "555-4321",
      "carrera": "Ingeniería de Sistemas",
      "programaId": 1,
      "activo": true,
      "fechaCreacion": "2024-01-15T10:00:00",
      "fechaModificacion": null,
      "nombreCompleto": "Ana Gómez",
      "email": "ana.gomez@ejemplo.com",
      "programa": "Programa General",
      "materiasInscritasCount": 3,
      "creditosActuales": 9
    },
    {
      "id": 2,
      "usuarioId": "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy",
      "identificacion": "2000000001",
      "telefono": "555-1001",
      "carrera": "Ingeniería de Sistemas",
      "programaId": 1,
      "activo": true,
      "fechaCreacion": "2024-01-15T11:00:00",
      "fechaModificacion": null,
      "nombreCompleto": "Laura Gómez",
      "email": "laura.gomez@estudiante.edu",
      "programa": "Programa General",
      "materiasInscritasCount": 2,
      "creditosActuales": 6
    }
  ]
}
```

#### 3.2.2. Obtener Estudiante por ID

Obtiene la información de un estudiante específico por su ID.

Acceso: `api/Estudiante/{id}`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.2.2.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del estudiante | Integer | Sí |

##### 3.2.2.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información del estudiante | Object |
| resultado.id | Identificador único del estudiante | Integer |
| resultado.usuarioId | ID del usuario asociado | GUID |
| resultado.identificacion | Número de identificación | String |
| resultado.telefono | Número telefónico | String |
| resultado.carrera | Nombre de la carrera | String |
| resultado.programaId | ID del programa asociado | Integer |
| resultado.activo | Estado de activación | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.fechaModificacion | Fecha de última modificación | DateTime |
| resultado.nombreCompleto | Nombre completo del estudiante | String |
| resultado.email | Correo electrónico | String |
| resultado.programa | Nombre del programa | String |
| resultado.materiasInscritasCount | Cantidad de materias inscritas | Integer |
| resultado.creditosActuales | Total de créditos actuales | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Estudiante obtenido",
  "detalle": "Se ha obtenido el estudiante 'Ana Gómez'",
  "resultado": {
    "id": 1,
    "usuarioId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "identificacion": "1098765432",
    "telefono": "555-4321",
    "carrera": "Ingeniería de Sistemas",
    "programaId": 1,
    "activo": true,
    "fechaCreacion": "2024-01-15T10:00:00",
    "fechaModificacion": null,
    "nombreCompleto": "Ana Gómez",
    "email": "ana.gomez@ejemplo.com",
    "programa": "Programa General",
    "materiasInscritasCount": 3,
    "creditosActuales": 9
  }
}
```

#### 3.2.3. Obtener Estudiantes Paginados

Obtiene una lista paginada de estudiantes con opciones de filtrado.

Acceso: `api/Estudiante/paginado`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.2.3.1. Parámetros de Consulta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** | **Valor Predeterminado** |
|------------|-----------------|----------|---------------|--------------------------|
| pagina | Número de página | Integer | No | 1 |
| elementosPorPagina | Cantidad de elementos por página | Integer | No | 10 |
| busqueda | Texto para filtrar resultados | String | No | null |

##### 3.2.3.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto con resultados paginados | Object |
| resultado.pagina | Número de página actual | Integer |
| resultado.elementosPorPagina | Cantidad de elementos por página | Integer |
| resultado.totalPaginas | Total de páginas disponibles | Integer |
| resultado.totalRegistros | Total de registros encontrados | Integer |
| resultado.lista | Lista de estudiantes | Array |
| resultado.lista[].id | Identificador único del estudiante | Integer |
| resultado.lista[].usuarioId | ID del usuario asociado | GUID |
| resultado.lista[].identificacion | Número de identificación | String |
| resultado.lista[].telefono | Número telefónico | String |
| resultado.lista[].carrera | Nombre de la carrera | String |
| resultado.lista[].programaId | ID del programa asociado | Integer |
| resultado.lista[].activo | Estado de activación | Boolean |
| resultado.lista[].fechaCreacion | Fecha de creación | DateTime |
| resultado.lista[].fechaModificacion | Fecha de última modificación | DateTime |
| resultado.lista[].nombreCompleto | Nombre completo del estudiante | String |
| resultado.lista[].email | Correo electrónico | String |
| resultado.lista[].programa | Nombre del programa | String |
| resultado.lista[].materiasInscritasCount | Cantidad de materias inscritas | Integer |
| resultado.lista[].creditosActuales | Total de créditos actuales | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Estudiantes obtenidos",
  "detalle": "Se han obtenido 2 estudiantes de un total de 2",
  "resultado": {
    "pagina": 1,
    "elementosPorPagina": 10,
    "totalPaginas": 1,
    "totalRegistros": 2,
    "lista": [
      {
        "id": 1,
        "usuarioId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
        "identificacion": "1098765432",
        "telefono": "555-4321",
        "carrera": "Ingeniería de Sistemas",
        "programaId": 1,
        "activo": true,
        "fechaCreacion": "2024-01-15T10:00:00",
        "fechaModificacion": null,
        "nombreCompleto": "Ana Gómez",
        "email": "ana.gomez@ejemplo.com",
        "programa": "Programa General",
        "materiasInscritasCount": 3,
        "creditosActuales": 9
      },
      {
        "id": 2,
        "usuarioId": "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy",
        "identificacion": "2000000001",
        "telefono": "555-1001",
        "carrera": "Ingeniería de Sistemas",
        "programaId": 1,
        "activo": true,
        "fechaCreacion": "2024-01-15T11:00:00",
        "fechaModificacion": null,
        "nombreCompleto": "Laura Gómez",
        "email": "laura.gomez@estudiante.edu",
        "programa": "Programa General",
        "materiasInscritasCount": 2,
        "creditosActuales": 6
      }
    ]
  }
}
```

#### 3.2.4. Crear Estudiante

Crea un nuevo estudiante en el sistema.

Acceso: `api/Estudiante`  
Formato: JSON  
Servicio: REST / POST  
Autenticación: JWT requerido

##### 3.2.4.1. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| usuarioId | ID del usuario asociado | GUID | Sí |
| identificacion | Número de identificación | String | Sí |
| telefono | Número telefónico | String | No |
| carrera | Nombre de la carrera | String | No |
| programaId | ID del programa asociado | Integer | Sí |
| activo | Estado de activación | Boolean | No |

Ejemplo de entrada:
```json
{
  "usuarioId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "identificacion": "1098765432",
  "telefono": "555-4321",
  "carrera": "Ingeniería de Sistemas",
  "programaId": 1,
  "activo": true
}
```

##### 3.2.4.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información del estudiante creado | Object |
| resultado.id | Identificador único del estudiante | Integer |
| resultado.usuarioId | ID del usuario asociado | GUID |
| resultado.identificacion | Número de identificación | String |
| resultado.telefono | Número telefónico | String |
| resultado.carrera | Nombre de la carrera | String |
| resultado.programaId | ID del programa asociado | Integer |
| resultado.activo | Estado de activación | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.nombreCompleto | Nombre completo del estudiante | String |
| resultado.email | Correo electrónico | String |
| resultado.programa | Nombre del programa | String |
| resultado.materiasInscritasCount | Cantidad de materias inscritas | Integer |
| resultado.creditosActuales | Total de créditos actuales | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Estudiante creado",
  "detalle": "El estudiante 'Ana Gómez' ha sido creado correctamente",
  "resultado": {
    "id": 1,
    "usuarioId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "identificacion": "1098765432",
    "telefono": "555-4321",
    "carrera": "Ingeniería de Sistemas",
    "programaId": 1,
    "activo": true,
    "fechaCreacion": "2024-05-12T14:30:00",
    "nombreCompleto": "Ana Gómez",
    "email": "ana.gomez@ejemplo.com",
    "programa": "Programa General",
    "materiasInscritasCount": 0,
    "creditosActuales": 0
  }
}
```

#### 3.2.5. Actualizar Estudiante

Actualiza un estudiante existente.

Acceso: `api/Estudiante/{id}`  
Formato: JSON  
Servicio: REST / PUT  
Autenticación: JWT requerido

##### 3.2.5.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del estudiante | Integer | Sí |

##### 3.2.5.2. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del estudiante (debe coincidir con el ID de la ruta) | Integer | No |
| usuarioId | ID del usuario asociado | GUID | Sí |
| identificacion | Número de identificación | String | Sí |
| telefono | Número telefónico | String | No |
| carrera | Nombre de la carrera | String | No |
| programaId | ID del programa asociado | Integer | Sí |
| activo | Estado de activación | Boolean | No |

Ejemplo de entrada:
```json
{
  "id": 1,
  "usuarioId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "identificacion": "1098765432",
  "telefono": "555-9876",
  "carrera": "Ingeniería de Software",
  "programaId": 1,
  "activo": true
}
```

##### 3.2.5.3. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información del estudiante actualizado | Object |
| resultado.id | Identificador único del estudiante | Integer |
| resultado.usuarioId | ID del usuario asociado | GUID |
| resultado.identificacion | Número de identificación | String |
| resultado.telefono | Número telefónico | String |
| resultado.carrera | Nombre de la carrera | String |
| resultado.programaId | ID del programa asociado | Integer |
| resultado.activo | Estado de activación | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.fechaModificacion | Fecha de modificación | DateTime |
| resultado.nombreCompleto | Nombre completo del estudiante | String |
| resultado.email | Correo electrónico | String |
| resultado.programa | Nombre del programa | String |
| resultado.materiasInscritasCount | Cantidad de materias inscritas | Integer |
| resultado.creditosActuales | Total de créditos actuales | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Estudiante actualizado",
  "detalle": "El estudiante 'Ana Gómez' ha sido actualizado correctamente",
  "resultado": {
    "id": 1,
    "usuarioId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "identificacion": "1098765432",
    "telefono": "555-9876",
    "carrera": "Ingeniería de Software",
    "programaId": 1,
    "activo": true,
    "fechaCreacion": "2024-01-15T10:00:00",
    "fechaModificacion": "2024-05-12T15:30:00",
    "nombreCompleto": "Ana Gómez",
    "email": "ana.gomez@ejemplo.com",
    "programa": "Programa General",
    "materiasInscritasCount": 3,
    "creditosActuales": 9
  }
}
```

#### 3.2.6. Eliminar Estudiante

Elimina (desactiva) un estudiante existente.

Acceso: `api/Estudiante/{id}`  
Formato: JSON  
Servicio: REST / DELETE  
Autenticación: JWT requerido

##### 3.2.6.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del estudiante | Integer | Sí |

##### 3.2.6.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto de resultado (nulo en este caso) | Object |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Estudiante eliminado",
  "detalle": "El estudiante 'Ana Gómez' ha sido eliminado correctamente",
  "resultado": null
}
```

#### 3.2.7. Obtener Compañeros de Clase

Obtiene una lista de compañeros de clase de un estudiante específico.

Acceso: `api/Estudiante/{id}/companeros`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.2.7.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del estudiante | Integer | Sí |

##### 3.2.7.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Lista de compañeros de clase | Array |
| resultado[].estudianteId | ID del estudiante de referencia | Integer |
| resultado[].materiaId | ID de la materia compartida | Integer |
| resultado[].materiaNombre | Nombre de la materia compartida | String |
| resultado[].companeroId | ID del compañero de clase | Integer |
| resultado[].companeroNombre | Nombre del compañero de clase | String |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Compañeros obtenidos",
  "detalle": "Se han obtenido 3 compañeros de clase",
  "resultado": [
    {
      "estudianteId": 1,
      "materiaId": 1,
      "materiaNombre": "Matemáticas I",
      "companeroId": 2,
      "companeroNombre": "Laura Gómez"
    },
    {
      "estudianteId": 1,
      "materiaId": 3,
      "materiaNombre": "Física I",
      "companeroId": 2,
      "companeroNombre": "Laura Gómez"
    },
    {
      "estudianteId": 1,
      "materiaId": 5,
      "materiaNombre": "Programación I",
      "companeroId": 3,
      "companeroNombre": "Carlos Rodríguez"
    }
  ]
}
```

### 3.3. Gestión de Profesores

#### 3.3.1. Obtener Todos los Profesores

Obtiene la lista de todos los profesores activos.

Acceso: `api/Profesor`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.3.1.1. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Lista de profesores | Array |
| resultado[].id | Identificador único del profesor | Integer |
| resultado[].usuarioId | ID del usuario asociado | GUID |
| resultado[].identificacion | Número de identificación | String |
| resultado[].telefono | Número telefónico | String |
| resultado[].departamento | Departamento académico | String |
| resultado[].activo | Estado de activación | Boolean |
| resultado[].fechaCreacion | Fecha de creación | DateTime |
| resultado[].fechaModificacion | Fecha de última modificación | DateTime |
| resultado[].nombreCompleto | Nombre completo del profesor | String |
| resultado[].email | Correo electrónico | String |
| resultado[].materias | Lista de materias asignadas | Array |
| resultado[].materiasCount | Cantidad de materias asignadas | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Profesores obtenidos",
  "detalle": "Se han obtenido 2 profesores",
  "resultado": [
    {
      "id": 1,
      "usuarioId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "identificacion": "1234567890",
      "telefono": "555-0001",
      "departamento": "Matemáticas",
      "activo": true,
      "fechaCreacion": "2024-01-15T10:00:00",
      "fechaModificacion": null,
      "nombreCompleto": "Juan García",
      "email": "juan.garcia@universidad.edu",
      "materias": [
        "MAT101 - Matemáticas I",
        "MAT102 - Cálculo I"
      ],
      "materiasCount": 2
    },
    {
      "id": 2,
      "usuarioId": "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy",
      "identificacion": "1234567891",
      "telefono": "555-0002",
      "departamento": "Ciencias",
      "activo": true,
      "fechaCreacion": "2024-01-15T11:00:00",
      "fechaModificacion": null,
      "nombreCompleto": "María López",
      "email": "maria.lopez@universidad.edu",
      "materias": [
        "FIS101 - Física I",
        "QUI101 - Química General"
      ],
      "materiasCount": 2
    }
  ]
}
```

#### 3.3.2. Obtener Profesor por ID

Obtiene la información de un profesor específico por su ID.

Acceso: `api/Profesor/{id}`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.3.2.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del profesor | Integer | Sí |

##### 3.3.2.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información del profesor | Object |
| resultado.id | Identificador único del profesor | Integer |
| resultado.usuarioId | ID del usuario asociado | GUID |
| resultado.identificacion | Número de identificación | String |
| resultado.telefono | Número telefónico | String |
| resultado.departamento | Departamento académico | String |
| resultado.activo | Estado de activación | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.fechaModificacion | Fecha de última modificación | DateTime |
| resultado.nombreCompleto | Nombre completo del profesor | String |
| resultado.email | Correo electrónico | String |
| resultado.materias | Lista de materias asignadas | Array |
| resultado.materiasCount | Cantidad de materias asignadas | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Profesor obtenido",
  "detalle": "Se ha obtenido el profesor 'Juan García'",
  "resultado": {
    "id": 1,
    "usuarioId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "identificacion": "1234567890",
    "telefono": "555-0001",
    "departamento": "Matemáticas",
    "activo": true,
    "fechaCreacion": "2024-01-15T10:00:00",
    "fechaModificacion": null,
    "nombreCompleto": "Juan García",
    "email": "juan.garcia@universidad.edu",
    "materias": [
      "MAT101 - Matemáticas I",
      "MAT102 - Cálculo I"
    ],
    "materiasCount": 2
  }
}
```

#### 3.3.3. Obtener Profesores Paginados

Obtiene una lista paginada de profesores con opciones de filtrado.

Acceso: `api/Profesor/paginado`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.3.3.1. Parámetros de Consulta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** | **Valor Predeterminado** |
|------------|-----------------|----------|---------------|--------------------------|
| pagina | Número de página | Integer | No | 1 |
| elementosPorPagina | Cantidad de elementos por página | Integer | No | 10 |
| busqueda | Texto para filtrar resultados | String | No | null |

##### 3.3.3.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto con resultados paginados | Object |
| resultado.pagina | Número de página actual | Integer |
| resultado.elementosPorPagina | Cantidad de elementos por página | Integer |
| resultado.totalPaginas | Total de páginas disponibles | Integer |
| resultado.totalRegistros | Total de registros encontrados | Integer |
| resultado.lista | Lista de profesores | Array |
| resultado.lista[].id | Identificador único del profesor | Integer |
| resultado.lista[].usuarioId | ID del usuario asociado | GUID |
| resultado.lista[].identificacion | Número de identificación | String |
| resultado.lista[].telefono | Número telefónico | String |
| resultado.lista[].departamento | Departamento académico | String |
| resultado.lista[].activo | Estado de activación | Boolean |
| resultado.lista[].fechaCreacion | Fecha de creación | DateTime |
| resultado.lista[].fechaModificacion | Fecha de última modificación | DateTime |
| resultado.lista[].nombreCompleto | Nombre completo del profesor | String |
| resultado.lista[].email | Correo electrónico | String |
| resultado.lista[].materias | Lista de materias asignadas | Array |
| resultado.lista[].materiasCount | Cantidad de materias asignadas | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Profesores obtenidos",
  "detalle": "Se han obtenido 2 profesores de un total de 5",
  "resultado": {
    "pagina": 1,
    "elementosPorPagina": 2,
    "totalPaginas": 3,
    "totalRegistros": 5,
    "lista": [
      {
        "id": 1,
        "usuarioId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
        "identificacion": "1234567890",
        "telefono": "555-0001",
        "departamento": "Matemáticas",
        "activo": true,
        "fechaCreacion": "2024-01-15T10:00:00",
        "fechaModificacion": null,
        "nombreCompleto": "Juan García",
        "email": "juan.garcia@universidad.edu",
        "materias": [
          "MAT101 - Matemáticas I",
          "MAT102 - Cálculo I"
        ],
        "materiasCount": 2
      },
      {
        "id": 2,
        "usuarioId": "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy",
        "identificacion": "1234567891",
        "telefono": "555-0002",
        "departamento": "Ciencias",
        "activo": true,
        "fechaCreacion": "2024-01-15T11:00:00",
        "fechaModificacion": null,
        "nombreCompleto": "María López",
        "email": "maria.lopez@universidad.edu",
        "materias": [
          "FIS101 - Física I",
          "QUI101 - Química General"
        ],
        "materiasCount": 2
      }
    ]
  }
}
```

#### 3.3.4. Crear Profesor

Crea un nuevo profesor en el sistema.

Acceso: `api/Profesor`  
Formato: JSON  
Servicio: REST / POST  
Autenticación: JWT requerido

##### 3.3.4.1. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| usuarioId | ID del usuario asociado | GUID | Sí |
| identificacion | Número de identificación | String | Sí |
| telefono | Número telefónico | String | No |
| departamento | Departamento académico | String | No |
| activo | Estado de activación | Boolean | No |

Ejemplo de entrada:
```json
{
  "usuarioId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "identificacion": "1234567895",
  "telefono": "555-0005",
  "departamento": "Computación",
  "activo": true
}
```

##### 3.3.4.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información del profesor creado | Object |
| resultado.id | Identificador único del profesor | Integer |
| resultado.usuarioId | ID del usuario asociado | GUID |
| resultado.identificacion | Número de identificación | String |
| resultado.telefono | Número telefónico | String |
| resultado.departamento | Departamento académico | String |
| resultado.activo | Estado de activación | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.nombreCompleto | Nombre completo del profesor | String |
| resultado.email | Correo electrónico | String |
| resultado.materias | Lista de materias asignadas | Array |
| resultado.materiasCount | Cantidad de materias asignadas | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Profesor creado",
  "detalle": "El profesor 'Roberto Sánchez' ha sido creado correctamente",
  "resultado": {
    "id": 6,
    "usuarioId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "identificacion": "1234567895",
    "telefono": "555-0005",
    "departamento": "Computación",
    "activo": true,
    "fechaCreacion": "2024-05-12T14:30:00",
    "nombreCompleto": "Roberto Sánchez",
    "email": "roberto.sanchez@universidad.edu",
    "materias": [],
    "materiasCount": 0
  }
}
```

#### 3.3.5. Actualizar Profesor

Actualiza un profesor existente.

Acceso: `api/Profesor/{id}`  
Formato: JSON  
Servicio: REST / PUT  
Autenticación: JWT requerido

##### 3.3.5.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del profesor | Integer | Sí |

##### 3.3.5.2. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del profesor (debe coincidir con el ID de la ruta) | Integer | No |
| usuarioId | ID del usuario asociado | GUID | Sí |
| identificacion | Número de identificación | String | Sí |
| telefono | Número telefónico | String | No |
| departamento | Departamento académico | String | No |
| activo | Estado de activación | Boolean | No |

Ejemplo de entrada:
```json
{
  "id": 6,
  "usuarioId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "identificacion": "1234567895",
  "telefono": "555-0099",
  "departamento": "Informática",
  "activo": true
}
```

##### 3.3.5.3. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información del profesor actualizado | Object |
| resultado.id | Identificador único del profesor | Integer |
| resultado.usuarioId | ID del usuario asociado | GUID |
| resultado.identificacion | Número de identificación | String |
| resultado.telefono | Número telefónico | String |
| resultado.departamento | Departamento académico | String |
| resultado.activo | Estado de activación | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.fechaModificacion | Fecha de modificación | DateTime |
| resultado.nombreCompleto | Nombre completo del profesor | String |
| resultado.email | Correo electrónico | String |
| resultado.materias | Lista de materias asignadas | Array |
| resultado.materiasCount | Cantidad de materias asignadas | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Profesor actualizado",
  "detalle": "El profesor 'Roberto Sánchez' ha sido actualizado correctamente",
  "resultado": {
    "id": 6,
    "usuarioId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "identificacion": "1234567895",
    "telefono": "555-0099",
    "departamento": "Informática",
    "activo": true,
    "fechaCreacion": "2024-05-12T14:30:00",
    "fechaModificacion": "2024-05-12T15:30:00",
    "nombreCompleto": "Roberto Sánchez",
    "email": "roberto.sanchez@universidad.edu",
    "materias": [],
    "materiasCount": 0
  }
}
```

#### 3.3.6. Eliminar Profesor

Elimina (desactiva) un profesor existente.

Acceso: `api/Profesor/{id}`  
Formato: JSON  
Servicio: REST / DELETE  
Autenticación: JWT requerido

##### 3.3.6.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del profesor | Integer | Sí |

##### 3.3.6.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto de resultado (nulo en este caso) | Object |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Profesor eliminado",
  "detalle": "El profesor 'Roberto Sánchez' ha sido eliminado correctamente",
  "resultado": null
}
```

### 3.4. Gestión de Materias

#### 3.4.1. Obtener Todas las Materias

Obtiene la lista de todas las materias activas.

Acceso: `api/Materia`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.4.1.1. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Lista de materias | Array |
| resultado[].id | Identificador único de la materia | Integer |
| resultado[].codigo | Código de la materia | String |
| resultado[].nombre | Nombre de la materia | String |
| resultado[].descripcion | Descripción de la materia | String |
| resultado[].creditos | Número de créditos | Integer |
| resultado[].activo | Estado de activación | Boolean |
| resultado[].fechaCreacion | Fecha de creación | DateTime |
| resultado[].fechaModificacion | Fecha de última modificación | DateTime |
| resultado[].creadoPorId | ID del usuario que creó el registro | GUID |
| resultado[].modificadoPorId | ID del usuario que modificó el registro | GUID |
| resultado[].creadoPor | Nombre del usuario que creó el registro | String |
| resultado[].modificadoPor | Nombre del usuario que modificó el registro | String |
| resultado[].inscripcionesCount | Cantidad de inscripciones | Integer |
| resultado[].profesores | Lista de profesores asignados | Array |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Materias obtenidas",
  "detalle": "Se han obtenido 3 materias",
  "resultado": [
    {
      "id": 1,
      "codigo": "MAT101",
      "nombre": "Matemáticas I",
      "descripcion": "Fundamentos de matemáticas",
      "creditos": 3,
      "activo": true,
      "fechaCreacion": "2024-01-15T10:00:00",
      "fechaModificacion": null,
      "creadoPorId": null,
      "modificadoPorId": null,
      "creadoPor": null,
      "modificadoPor": null,
      "inscripcionesCount": 2,
      "profesores": [
        "Juan García"
      ]
    },
    {
      "id": 2,
      "codigo": "MAT102",
      "nombre": "Cálculo I",
      "descripcion": "Introducción al cálculo diferencial",
      "creditos": 3,
      "activo": true,
      "fechaCreacion": "2024-01-15T10:00:00",
      "fechaModificacion": null,
      "creadoPorId": null,
      "modificadoPorId": null,
      "creadoPor": null,
      "modificadoPor": null,
      "inscripcionesCount": 1,
      "profesores": [
        "Juan García"
      ]
    },
    {
      "id": 3,
      "codigo": "FIS101",
      "nombre": "Física I",
      "descripcion": "Mecánica clásica",
      "creditos": 3,
      "activo": true,
      "fechaCreacion": "2024-01-15T10:00:00",
      "fechaModificacion": null,
      "creadoPorId": null,
      "modificadoPorId": null,
      "creadoPor": null,
      "modificadoPor": null,
      "inscripcionesCount": 2,
      "profesores": [
        "María López"
      ]
    }
  ]
}
```

#### 3.4.2. Obtener Materia por ID

Obtiene la información de una materia específica por su ID.

Acceso: `api/Materia/{id}`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.4.2.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID de la materia | Integer | Sí |

##### 3.4.2.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información de la materia | Object |
| resultado.id | Identificador único de la materia | Integer |
| resultado.codigo | Código de la materia | String |
| resultado.nombre | Nombre de la materia | String |
| resultado.descripcion | Descripción de la materia | String |
| resultado.creditos | Número de créditos | Integer |
| resultado.activo | Estado de activación | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.fechaModificacion | Fecha de última modificación | DateTime |
| resultado.creadoPorId | ID del usuario que creó el registro | GUID |
| resultado.modificadoPorId | ID del usuario que modificó el registro | GUID |
| resultado.creadoPor | Nombre del usuario que creó el registro | String |
| resultado.modificadoPor | Nombre del usuario que modificó el registro | String |
| resultado.inscripcionesCount | Cantidad de inscripciones | Integer |
| resultado.profesores | Lista de profesores asignados | Array |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Materia obtenida",
  "detalle": "Se ha obtenido la materia 'MAT101 - Matemáticas I'",
  "resultado": {
    "id": 1,
    "codigo": "MAT101",
    "nombre": "Matemáticas I",
    "descripcion": "Fundamentos de matemáticas",
    "creditos": 3,
    "activo": true,
    "fechaCreacion": "2024-01-15T10:00:00",
    "fechaModificacion": null,
    "creadoPorId": null,
    "modificadoPorId": null,
    "creadoPor": null,
    "modificadoPor": null,
    "inscripcionesCount": 2,
    "profesores": [
      "Juan García"
    ]
  }
}
```

#### 3.4.3. Obtener Materias Paginadas

Obtiene una lista paginada de materias con opciones de filtrado.

Acceso: `api/Materia/paginado`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.4.3.1. Parámetros de Consulta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** | **Valor Predeterminado** |
|------------|-----------------|----------|---------------|--------------------------|
| pagina | Número de página | Integer | No | 1 |
| elementosPorPagina | Cantidad de elementos por página | Integer | No | 10 |
| busqueda | Texto para filtrar resultados | String | No | null |

##### 3.4.3.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto con resultados paginados | Object |
| resultado.pagina | Número de página actual | Integer |
| resultado.elementosPorPagina | Cantidad de elementos por página | Integer |
| resultado.totalPaginas | Total de páginas disponibles | Integer |
| resultado.totalRegistros | Total de registros encontrados | Integer |
| resultado.lista | Lista de materias | Array |
| resultado.lista[].id | Identificador único de la materia | Integer |
| resultado.lista[].codigo | Código de la materia | String |
| resultado.lista[].nombre | Nombre de la materia | String |
| resultado.lista[].descripcion | Descripción de la materia | String |
| resultado.lista[].creditos | Número de créditos | Integer |
| resultado.lista[].activo | Estado de activación | Boolean |
| resultado.lista[].fechaCreacion | Fecha de creación | DateTime |
| resultado.lista[].fechaModificacion | Fecha de última modificación | DateTime |
| resultado.lista[].creadoPorId | ID del usuario que creó el registro | GUID |
| resultado.lista[].modificadoPorId | ID del usuario que modificó el registro | GUID |
| resultado.lista[].creadoPor | Nombre del usuario que creó el registro | String |
| resultado.lista[].modificadoPor | Nombre del usuario que modificó el registro | String |
| resultado.lista[].inscripcionesCount | Cantidad de inscripciones | Integer |
| resultado.lista[].profesores | Lista de profesores asignados | Array |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Materias obtenidas",
  "detalle": "Se han obtenido 3 materias de un total de 10",
  "resultado": {
    "pagina": 1,
    "elementosPorPagina": 3,
    "totalPaginas": 4,
    "totalRegistros": 10,
    "lista": [
      {
        "id": 1,
        "codigo": "MAT101",
        "nombre": "Matemáticas I",
        "descripcion": "Fundamentos de matemáticas",
        "creditos": 3,
        "activo": true,
        "fechaCreacion": "2024-01-15T10:00:00",
        "fechaModificacion": null,
        "creadoPorId": null,
        "modificadoPorId": null,
        "creadoPor": null,
        "modificadoPor": null,
        "inscripcionesCount": 2,
        "profesores": [
          "Juan García"
        ]
      },
      {
        "id": 2,
        "codigo": "MAT102",
        "nombre": "Cálculo I",
        "descripcion": "Introducción al cálculo diferencial",
        "creditos": 3,
        "activo": true,
        "fechaCreacion": "2024-01-15T10:00:00",
        "fechaModificacion": null,
        "creadoPorId": null,
        "modificadoPorId": null,
        "creadoPor": null,
        "modificadoPor": null,
        "inscripcionesCount": 1,
        "profesores": [
          "Juan García"
        ]
      },
      {
        "id": 3,
        "codigo": "FIS101",
        "nombre": "Física I",
        "descripcion": "Mecánica clásica",
        "creditos": 3,
        "activo": true,
        "fechaCreacion": "2024-01-15T10:00:00",
        "fechaModificacion": null,
        "creadoPorId": null,
        "modificadoPorId": null,
        "creadoPor": null,
        "modificadoPor": null,
        "inscripcionesCount": 2,
        "profesores": [
          "María López"
        ]
      }
    ]
  }
}
```

#### 3.4.4. Crear Materia

Crea una nueva materia en el sistema.

Acceso: `api/Materia`  
Formato: JSON  
Servicio: REST / POST  
Autenticación: JWT requerido

##### 3.4.4.1. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| codigo | Código de la materia | String | Sí |
| nombre | Nombre de la materia | String | Sí |
| descripcion | Descripción de la materia | String | No |
| creditos | Número de créditos | Integer | No |
| activo | Estado de activación | Boolean | No |

Ejemplo de entrada:
```json
{
  "codigo": "CS101",
  "nombre": "Introducción a la Ciencia de Datos",
  "descripcion": "Fundamentos de ciencia de datos y analítica",
  "creditos": 4,
  "activo": true
}
```

##### 3.4.4.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información de la materia creada | Object |
| resultado.id | Identificador único de la materia | Integer |
| resultado.codigo | Código de la materia | String |
| resultado.nombre | Nombre de la materia | String |
| resultado.descripcion | Descripción de la materia | String |
| resultado.creditos | Número de créditos | Integer |
| resultado.activo | Estado de activación | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.creadoPorId | ID del usuario que creó el registro | GUID |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Materia creada",
  "detalle": "La materia 'CS101 - Introducción a la Ciencia de Datos' ha sido creada correctamente",
  "resultado": {
    "id": 11,
    "codigo": "CS101",
    "nombre": "Introducción a la Ciencia de Datos",
    "descripcion": "Fundamentos de ciencia de datos y analítica",
    "creditos": 4,
    "activo": true,
    "fechaCreacion": "2024-05-12T14:30:00",
    "creadoPorId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  }
}
```

#### 3.4.5. Actualizar Materia

Actualiza una materia existente.

Acceso: `api/Materia/{id}`  
Formato: JSON  
Servicio: REST / PUT  
Autenticación: JWT requerido

##### 3.4.5.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID de la materia | Integer | Sí |

##### 3.4.5.2. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID de la materia (debe coincidir con el ID de la ruta) | Integer | No |
| codigo | Código de la materia | String | Sí |
| nombre | Nombre de la materia | String | Sí |
| descripcion | Descripción de la materia | String | No |
| creditos | Número de créditos | Integer | No |
| activo | Estado de activación | Boolean | No |

Ejemplo de entrada:
```json
{
  "id": 11,
  "codigo": "CS101",
  "nombre": "Fundamentos de Ciencia de Datos",
  "descripcion": "Introducción a la ciencia de datos: conceptos, métodos y herramientas",
  "creditos": 3,
  "activo": true
}
```

##### 3.4.5.3. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información de la materia actualizada | Object |
| resultado.id | Identificador único de la materia | Integer |
| resultado.codigo | Código de la materia | String |
| resultado.nombre | Nombre de la materia | String |
| resultado.descripcion | Descripción de la materia | String |
| resultado.creditos | Número de créditos | Integer |
| resultado.activo | Estado de activación | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.fechaModificacion | Fecha de modificación | DateTime |
| resultado.creadoPorId | ID del usuario que creó el registro | GUID |
| resultado.modificadoPorId | ID del usuario que modificó el registro | GUID |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Materia actualizada",
  "detalle": "La materia 'CS101 - Fundamentos de Ciencia de Datos' ha sido actualizada correctamente",
  "resultado": {
    "id": 11,
    "codigo": "CS101",
    "nombre": "Fundamentos de Ciencia de Datos",
    "descripcion": "Introducción a la ciencia de datos: conceptos, métodos y herramientas",
    "creditos": 3,
    "activo": true,
    "fechaCreacion": "2024-05-12T14:30:00",
    "fechaModificacion": "2024-05-12T15:30:00",
    "creadoPorId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "modificadoPorId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  }
}
```

#### 3.4.6. Eliminar Materia

Elimina (desactiva) una materia existente.

Acceso: `api/Materia/{id}`  
Formato: JSON  
Servicio: REST / DELETE  
Autenticación: JWT requerido

##### 3.4.6.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID de la materia | Integer | Sí |

##### 3.4.6.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto de resultado (nulo en este caso) | Object |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Materia eliminada",
  "detalle": "La materia 'CS101 - Fundamentos de Ciencia de Datos' ha sido eliminada correctamente",
  "resultado": null
}
```

### 3.5. Gestión de Programas

#### 3.5.1. Obtener Todos los Programas

Obtiene la lista de todos los programas activos.

Acceso: `api/Programa`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.5.1.1. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Lista de programas | Array |
| resultado[].id | Identificador único del programa | Integer |
| resultado[].nombre | Nombre del programa | String |
| resultado[].descripcion | Descripción del programa | String |
| resultado[].creditosMinimos | Créditos mínimos requeridos | Integer |
| resultado[].creditosMaximos | Créditos máximos permitidos | Integer |
| resultado[].activo | Estado de activación | Boolean |
| resultado[].fechaCreacion | Fecha de creación | DateTime |
| resultado[].fechaModificacion | Fecha de última modificación | DateTime |
| resultado[].creadoPorId | ID del usuario que creó el registro | GUID |
| resultado[].modificadoPorId | ID del usuario que modificó el registro | GUID |
| resultado[].creadoPor | Nombre del usuario que creó el registro | String |
| resultado[].modificadoPor | Nombre del usuario que modificó el registro | String |
| resultado[].estudiantesCount | Cantidad de estudiantes inscritos | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Programas obtenidos",
  "detalle": "Se han obtenido 2 programas",
  "resultado": [
    {
      "id": 1,
      "nombre": "Programa General",
      "descripcion": "Programa general de créditos para estudiantes",
      "creditosMinimos": 9,
      "creditosMaximos": 9,
      "activo": true,
      "fechaCreacion": "2024-01-15T10:00:00",
      "fechaModificacion": null,
      "creadoPorId": null,
      "modificadoPorId": null,
      "creadoPor": null,
      "modificadoPor": null,
      "estudiantesCount": 2
    },
    {
      "id": 2,
      "nombre": "Ingeniería de Sistemas",
      "descripcion": "Programa de grado en ingeniería de sistemas",
      "creditosMinimos": 18,
      "creditosMaximos": 21,
      "activo": true,
      "fechaCreacion": "2024-01-15T10:00:00",
      "fechaModificacion": null,
      "creadoPorId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "modificadoPorId": null,
      "creadoPor": "Administrador Sistema",
      "modificadoPor": null,
      "estudiantesCount": 3
    }
  ]
}
```

#### 3.5.2. Obtener Programa por ID

Obtiene la información de un programa específico por su ID.

Acceso: `api/Programa/{id}`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.5.2.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del programa | Integer | Sí |

##### 3.5.2.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información del programa | Object |
| resultado.id | Identificador único del programa | Integer |
| resultado.nombre | Nombre del programa | String |
| resultado.descripcion | Descripción del programa | String |
| resultado.creditosMinimos | Créditos mínimos requeridos | Integer |
| resultado.creditosMaximos | Créditos máximos permitidos | Integer |
| resultado.activo | Estado de activación | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.fechaModificacion | Fecha de última modificación | DateTime |
| resultado.creadoPorId | ID del usuario que creó el registro | GUID |
| resultado.modificadoPorId | ID del usuario que modificó el registro | GUID |
| resultado.creadoPor | Nombre del usuario que creó el registro | String |
| resultado.modificadoPor | Nombre del usuario que modificó el registro | String |
| resultado.estudiantesCount | Cantidad de estudiantes inscritos | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Programa obtenido",
  "detalle": "Se ha obtenido el programa 'Ingeniería de Sistemas'",
  "resultado": {
    "id": 2,
    "nombre": "Ingeniería de Sistemas",
    "descripcion": "Programa de grado en ingeniería de sistemas",
    "creditosMinimos": 18,
    "creditosMaximos": 21,
    "activo": true,
    "fechaCreacion": "2024-01-15T10:00:00",
    "fechaModificacion": null,
    "creadoPorId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "modificadoPorId": null,
    "creadoPor": "Administrador Sistema",
    "modificadoPor": null,
    "estudiantesCount": 3
  }
}
```

#### 3.5.3. Obtener Programas Paginados

Obtiene una lista paginada de programas con opciones de filtrado.

Acceso: `api/Programa/paginado`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.5.3.1. Parámetros de Consulta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** | **Valor Predeterminado** |
|------------|-----------------|----------|---------------|--------------------------|
| pagina | Número de página | Integer | No | 1 |
| elementosPorPagina | Cantidad de elementos por página | Integer | No | 10 |
| busqueda | Texto para filtrar resultados | String | No | null |

##### 3.5.3.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto con resultados paginados | Object |
| resultado.pagina | Número de página actual | Integer |
| resultado.elementosPorPagina | Cantidad de elementos por página | Integer |
| resultado.totalPaginas | Total de páginas disponibles | Integer |
| resultado.totalRegistros | Total de registros encontrados | Integer |
| resultado.lista | Lista de programas | Array |
| resultado.lista[].id | Identificador único del programa | Integer |
| resultado.lista[].nombre | Nombre del programa | String |
| resultado.lista[].descripcion | Descripción del programa | String |
| resultado.lista[].creditosMinimos | Créditos mínimos requeridos | Integer |
| resultado.lista[].creditosMaximos | Créditos máximos permitidos | Integer |
| resultado.lista[].activo | Estado de activación | Boolean |
| resultado.lista[].fechaCreacion | Fecha de creación | DateTime |
| resultado.lista[].fechaModificacion | Fecha de última modificación | DateTime |
| resultado.lista[].creadoPorId | ID del usuario que creó el registro | GUID |
| resultado.lista[].modificadoPorId | ID del usuario que modificó el registro | GUID |
| resultado.lista[].creadoPor | Nombre del usuario que creó el registro | String |
| resultado.lista[].modificadoPor | Nombre del usuario que modificó el registro | String |
| resultado.lista[].estudiantesCount | Cantidad de estudiantes inscritos | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Programas obtenidos",
  "detalle": "Se han obtenido 2 programas de un total de 2",
  "resultado": {
    "pagina": 1,
    "elementosPorPagina": 10,
    "totalPaginas": 1,
    "totalRegistros": 2,
    "lista": [
      {
        "id": 1,
        "nombre": "Programa General",
        "descripcion": "Programa general de créditos para estudiantes",
        "creditosMinimos": 9,
        "creditosMaximos": 9,
        "activo": true,
        "fechaCreacion": "2024-01-15T10:00:00",
        "fechaModificacion": null,
        "creadoPorId": null,
        "modificadoPorId": null,
        "creadoPor": null,
        "modificadoPor": null,
        "estudiantesCount": 2
      },
      {
        "id": 2,
        "nombre": "Ingeniería de Sistemas",
        "descripcion": "Programa de grado en ingeniería de sistemas",
        "creditosMinimos": 18,
        "creditosMaximos": 21,
        "activo": true,
        "fechaCreacion": "2024-01-15T10:00:00",
        "fechaModificacion": null,
        "creadoPorId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
        "modificadoPorId": null,
        "creadoPor": "Administrador Sistema",
        "modificadoPor": null,
        "estudiantesCount": 3
      }
    ]
  }
}
```

#### 3.5.4. Crear Programa

Crea un nuevo programa académico.

Acceso: `api/Programa`  
Formato: JSON  
Servicio: REST / POST  
Autenticación: JWT requerido

##### 3.5.4.1. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| nombre | Nombre del programa | String | Sí |
| descripcion | Descripción del programa | String | No |
| creditosMinimos | Créditos mínimos requeridos | Integer | No |
| creditosMaximos | Créditos máximos permitidos | Integer | No |
| activo | Estado de activación | Boolean | No |

Ejemplo de entrada:
```json
{
  "nombre": "Ingeniería de Software",
  "descripcion": "Programa especializado en desarrollo de software",
  "creditosMinimos": 12,
  "creditosMaximos": 18,
  "activo": true
}
```

##### 3.5.4.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información del programa creado | Object |
| resultado.id | Identificador único del programa | Integer |
| resultado.nombre | Nombre del programa | String |
| resultado.descripcion | Descripción del programa | String |
| resultado.creditosMinimos | Créditos mínimos requeridos | Integer |
| resultado.creditosMaximos | Créditos máximos permitidos | Integer |
| resultado.activo | Estado de activación | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.creadoPorId | ID del usuario que creó el registro | GUID |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Programa creado",
  "detalle": "El programa 'Ingeniería de Software' ha sido creado correctamente",
  "resultado": {
    "id": 3,
    "nombre": "Ingeniería de Software",
    "descripcion": "Programa especializado en desarrollo de software",
    "creditosMinimos": 12,
    "creditosMaximos": 18,
    "activo": true,
    "fechaCreacion": "2024-05-12T14:30:00",
    "creadoPorId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  }
}
```

#### 3.5.5. Actualizar Programa

Actualiza un programa académico existente.

Acceso: `api/Programa/{id}`  
Formato: JSON  
Servicio: REST / PUT  
Autenticación: JWT requerido

##### 3.5.5.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del programa | Integer | Sí |

##### 3.5.5.2. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del programa (debe coincidir con el ID de la ruta) | Integer | No |
| nombre | Nombre del programa | String | Sí |
| descripcion | Descripción del programa | String | No |
| creditosMinimos | Créditos mínimos requeridos | Integer | No |
| creditosMaximos | Créditos máximos permitidos | Integer | No |
| activo | Estado de activación | Boolean | No |

Ejemplo de entrada:
```json
{
  "id": 3,
  "nombre": "Ingeniería de Software y Sistemas",
  "descripcion": "Programa especializado en desarrollo de software y sistemas informáticos",
  "creditosMinimos": 15,
  "creditosMaximos": 21,
  "activo": true
}
```

##### 3.5.5.3. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información del programa actualizado | Object |
| resultado.id | Identificador único del programa | Integer |
| resultado.nombre | Nombre del programa | String |
| resultado.descripcion | Descripción del programa | String |
| resultado.creditosMinimos | Créditos mínimos requeridos | Integer |
| resultado.creditosMaximos | Créditos máximos permitidos | Integer |
| resultado.activo | Estado de activación | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.fechaModificacion | Fecha de modificación | DateTime |
| resultado.creadoPorId | ID del usuario que creó el registro | GUID |
| resultado.modificadoPorId | ID del usuario que modificó el registro | GUID |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Programa actualizado",
  "detalle": "El programa 'Ingeniería de Software y Sistemas' ha sido actualizado correctamente",
  "resultado": {
    "id": 3,
    "nombre": "Ingeniería de Software y Sistemas",
    "descripcion": "Programa especializado en desarrollo de software y sistemas informáticos",
    "creditosMinimos": 15,
    "creditosMaximos": 21,
    "activo": true,
    "fechaCreacion": "2024-05-12T14:30:00",
    "fechaModificacion": "2024-05-12T15:30:00",
    "creadoPorId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "modificadoPorId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  }
}
```

#### 3.5.6. Eliminar Programa

Elimina (desactiva) un programa existente.

Acceso: `api/Programa/{id}`  
Formato: JSON  
Servicio: REST / DELETE  
Autenticación: JWT requerido

##### 3.5.6.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del programa | Integer | Sí |

##### 3.5.6.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto de resultado (nulo en este caso) | Object |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Programa eliminado",
  "detalle": "El programa 'Ingeniería de Software y Sistemas' ha sido eliminado correctamente",
  "resultado": null
}
```

### 3.6. Inscripciones de Estudiantes

#### 3.6.1. Obtener Inscripciones por Estudiante

Obtiene todas las inscripciones de un estudiante específico.

Acceso: `api/InscripcionEstudiante/estudiante/{estudianteId}`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.6.1.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| estudianteId | ID del estudiante | Integer | Sí |

##### 3.6.1.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Lista de inscripciones | Array |
| resultado[].id | Identificador único de la inscripción | Integer |
| resultado[].estudianteId | ID del estudiante | Integer |
| resultado[].materiaId | ID de la materia | Integer |
| resultado[].fechaInscripcion | Fecha de inscripción | DateTime |
| resultado[].estado | Estado de la inscripción | String |
| resultado[].calificacion | Calificación obtenida | Decimal |
| resultado[].fechaModificacion | Fecha de última modificación | DateTime |
| resultado[].activo | Estado de activación | Boolean |
| resultado[].estudianteNombre | Nombre del estudiante | String |
| resultado[].materiaCodigoyNombre | Código y nombre de la materia | String |
| resultado[].credMateria | Créditos de la materia | Integer |
| resultado[].profesores | Lista de profesores asignados | Array |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Inscripciones obtenidas",
  "detalle": "Se han obtenido 3 inscripciones del estudiante",
  "resultado": [
    {
      "id": 1,
      "estudianteId": 1,
      "materiaId": 1,
      "fechaInscripcion": "2024-01-15T10:00:00",
      "estado": "Inscrito",
      "calificacion": null,
      "fechaModificacion": null,
      "activo": true,
      "estudianteNombre": "Laura Gómez",
      "materiaCodigoyNombre": "MAT101 - Matemáticas I",
      "credMateria": 3,
      "profesores": [
        "Juan García"
      ]
    },
    {
      "id": 2,
      "estudianteId": 1,
      "materiaId": 3,
      "fechaInscripcion": "2024-01-15T10:15:00",
      "estado": "Inscrito",
      "calificacion": null,
      "fechaModificacion": null,
      "activo": true,
      "estudianteNombre": "Laura Gómez",
      "materiaCodigoyNombre": "FIS101 - Física I",
      "credMateria": 3,
      "profesores": [
        "María López"
      ]
    },
    {
      "id": 3,
      "estudianteId": 1,
      "materiaId": 5,
      "fechaInscripcion": "2024-01-15T10:30:00",
      "estado": "Inscrito",
      "calificacion": null,
      "fechaModificacion": null,
      "activo": true,
      "estudianteNombre": "Laura Gómez",
      "materiaCodigoyNombre": "PRG101 - Programación I",
      "credMateria": 3,
      "profesores": [
        "Carlos Martín"
      ]
    }
  ]
}
```

#### 3.6.2. Obtener Inscripciones por Materia

Obtiene todas las inscripciones de una materia específica.

Acceso: `api/InscripcionEstudiante/materia/{materiaId}`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.6.2.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| materiaId | ID de la materia | Integer | Sí |

##### 3.6.2.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Lista de inscripciones | Array |
| resultado[].id | Identificador único de la inscripción | Integer |
| resultado[].estudianteId | ID del estudiante | Integer |
| resultado[].materiaId | ID de la materia | Integer |
| resultado[].fechaInscripcion | Fecha de inscripción | DateTime |
| resultado[].estado | Estado de la inscripción | String |
| resultado[].calificacion | Calificación obtenida | Decimal |
| resultado[].fechaModificacion | Fecha de última modificación | DateTime |
| resultado[].activo | Estado de activación | Boolean |
| resultado[].estudianteNombre | Nombre del estudiante | String |
| resultado[].materiaCodigoyNombre | Código y nombre de la materia | String |
| resultado[].credMateria | Créditos de la materia | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Inscripciones obtenidas",
  "detalle": "Se han obtenido 2 inscripciones de la materia",
  "resultado": [
    {
      "id": 1,
      "estudianteId": 1,
      "materiaId": 1,
      "fechaInscripcion": "2024-01-15T10:00:00",
      "estado": "Inscrito",
      "calificacion": null,
      "fechaModificacion": null,
      "activo": true,
      "estudianteNombre": "Laura Gómez",
      "materiaCodigoyNombre": "MAT101 - Matemáticas I",
      "credMateria": 3
    },
    {
      "id": 4,
      "estudianteId": 2,
      "materiaId": 1,
      "fechaInscripcion": "2024-01-15T11:00:00",
      "estado": "Inscrito",
      "calificacion": null,
      "fechaModificacion": null,
      "activo": true,
      "estudianteNombre": "Carlos Rodríguez",
      "materiaCodigoyNombre": "MAT101 - Matemáticas I",
      "credMateria": 3
    }
  ]
}
```

#### 3.6.3. Obtener Inscripción por ID

Obtiene una inscripción específica por su ID.

Acceso: `api/InscripcionEstudiante/{id}`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.6.3.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID de la inscripción | Integer | Sí |

##### 3.6.3.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información de la inscripción | Object |
| resultado.id | Identificador único de la inscripción | Integer |
| resultado.estudianteId | ID del estudiante | Integer |
| resultado.materiaId | ID de la materia | Integer |
| resultado.fechaInscripcion | Fecha de inscripción | DateTime |
| resultado.estado | Estado de la inscripción | String |
| resultado.calificacion | Calificación obtenida | Decimal |
| resultado.fechaModificacion | Fecha de última modificación | DateTime |
| resultado.activo | Estado de activación | Boolean |
| resultado.estudianteNombre | Nombre del estudiante | String |
| resultado.materiaCodigoyNombre | Código y nombre de la materia | String |
| resultado.credMateria | Créditos de la materia | Integer |
| resultado.profesores | Lista de profesores asignados | Array |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Inscripcion obtenida",
  "detalle": "Se ha obtenido la inscripcion correctamente",
  "resultado": {
    "id": 1,
    "estudianteId": 1,
    "materiaId": 1,
    "fechaInscripcion": "2024-01-15T10:00:00",
    "estado": "Inscrito",
    "calificacion": null,
    "fechaModificacion": null,
    "activo": true,
    "estudianteNombre": "Laura Gómez",
    "materiaCodigoyNombre": "MAT101 - Matemáticas I",
    "credMateria": 3,
    "profesores": [
      "Juan García"
    ]
  }
}
```

#### 3.6.4. Obtener Inscripciones Paginadas

Obtiene una lista paginada de inscripciones con opciones de filtrado.

Acceso: `api/InscripcionEstudiante/paginado`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.6.4.1. Parámetros de Consulta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** | **Valor Predeterminado** |
|------------|-----------------|----------|---------------|--------------------------|
| pagina | Número de página | Integer | No | 1 |
| elementosPorPagina | Cantidad de elementos por página | Integer | No | 10 |
| estudianteId | ID del estudiante para filtrar | Integer | No | null |
| materiaId | ID de la materia para filtrar | Integer | No | null |

##### 3.6.4.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto con resultados paginados | Object |
| resultado.pagina | Número de página actual | Integer |
| resultado.elementosPorPagina | Cantidad de elementos por página | Integer |
| resultado.totalPaginas | Total de páginas disponibles | Integer |
| resultado.totalRegistros | Total de registros encontrados | Integer |
| resultado.lista | Lista de inscripciones | Array |
| resultado.lista[].id | Identificador único de la inscripción | Integer |
| resultado.lista[].estudianteId | ID del estudiante | Integer |
| resultado.lista[].materiaId | ID de la materia | Integer |
| resultado.lista[].fechaInscripcion | Fecha de inscripción | DateTime |
| resultado.lista[].estado | Estado de la inscripción | String |
| resultado.lista[].calificacion | Calificación obtenida | Decimal |
| resultado.lista[].fechaModificacion | Fecha de última modificación | DateTime |
| resultado.lista[].activo | Estado de activación | Boolean |
| resultado.lista[].estudianteNombre | Nombre del estudiante | String |
| resultado.lista[].materiaCodigoyNombre | Código y nombre de la materia | String |
| resultado.lista[].credMateria | Créditos de la materia | Integer |
| resultado.lista[].profesores | Lista de profesores asignados | Array |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Inscripciones obtenidas",
  "detalle": "Se han obtenido 3 inscripciones de un total de 10",
  "resultado": {
    "pagina": 1,
    "elementosPorPagina": 3,
    "totalPaginas": 4,
    "totalRegistros": 10,
    "lista": [
      {
        "id": 1,
        "estudianteId": 1,
        "materiaId": 1,
        "fechaInscripcion": "2024-01-15T10:00:00",
        "estado": "Inscrito",
        "calificacion": null,
        "fechaModificacion": null,
        "activo": true,
        "estudianteNombre": "Laura Gómez",
        "materiaCodigoyNombre": "MAT101 - Matemáticas I",
        "credMateria": 3,
        "profesores": [
          "Juan García"
        ]
      },
      {
        "id": 2,
        "estudianteId": 1,
        "materiaId": 3,
        "fechaInscripcion": "2024-01-15T10:15:00",
        "estado": "Inscrito",
        "calificacion": null,
        "fechaModificacion": null,
        "activo": true,
        "estudianteNombre": "Laura Gómez",
        "materiaCodigoyNombre": "FIS101 - Física I",
        "credMateria": 3,
        "profesores": [
          "María López"
        ]
      },
      {
        "id": 3,
        "estudianteId": 1,
        "materiaId": 5,
        "fechaInscripcion": "2024-01-15T10:30:00",
        "estado": "Inscrito",
        "calificacion": null,
        "fechaModificacion": null,
        "activo": true,
        "estudianteNombre": "Laura Gómez",
        "materiaCodigoyNombre": "PRG101 - Programación I",
        "credMateria": 3,
        "profesores": [
          "Carlos Martín"
        ]
      }
    ]
  }
}
```

#### 3.6.5. Inscribir Estudiante

Crea una nueva inscripción de un estudiante en una materia.

Acceso: `api/InscripcionEstudiante`  
Formato: JSON  
Servicio: REST / POST  
Autenticación: JWT requerido

##### 3.6.5.1. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| estudianteId | ID del estudiante | Integer | Sí |
| materiaId | ID de la materia | Integer | Sí |
| estado | Estado de la inscripción | String | Sí |
| activo | Estado de activación | Boolean | No |

Ejemplo de entrada:
```json
{
  "estudianteId": 1,
  "materiaId": 7,
  "estado": "Inscrito",
  "activo": true
}
```

##### 3.6.5.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información de la inscripción creada | Object |
| resultado.id | Identificador único de la inscripción | Integer |
| resultado.estudianteId | ID del estudiante | Integer |
| resultado.materiaId | ID de la materia | Integer |
| resultado.fechaInscripcion | Fecha de inscripción | DateTime |
| resultado.estado | Estado de la inscripción | String |
| resultado.calificacion | Calificación obtenida | Decimal |
| resultado.fechaModificacion | Fecha de última modificación | DateTime |
| resultado.activo | Estado de activación | Boolean |
| resultado.estudianteNombre | Nombre del estudiante | String |
| resultado.materiaCodigoyNombre | Código y nombre de la materia | String |
| resultado.credMateria | Créditos de la materia | Integer |
| resultado.profesores | Lista de profesores asignados | Array |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Inscripcion exitosa",
  "detalle": "El estudiante ha sido inscrito en BIO101 - Biología General",
  "resultado": {
    "id": 4,
    "estudianteId": 1,
    "materiaId": 7,
    "fechaInscripcion": "2024-05-12T14:30:00",
    "estado": "Inscrito",
    "calificacion": null,
    "fechaModificacion": null,
    "activo": true,
    "estudianteNombre": "Laura Gómez",
    "materiaCodigoyNombre": "BIO101 - Biología General",
    "credMateria": 3,
    "profesores": [
      "Ana Ruiz"
    ]
  }
}
```

#### 3.6.6. Calificar Inscripción

Asigna una calificación a una inscripción existente.

Acceso: `api/InscripcionEstudiante/{id}/calificar`  
Formato: JSON  
Servicio: REST / PUT  
Autenticación: JWT requerido

##### 3.6.6.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID de la inscripción | Integer | Sí |

##### 3.6.6.2. Parámetros de Consulta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| calificacion | Calificación (0.0 - 5.0) | Decimal | Sí |

##### 3.6.6.3. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información de la inscripción actualizada | Object |
| resultado.id | Identificador único de la inscripción | Integer |
| resultado.estudianteId | ID del estudiante | Integer |
| resultado.materiaId | ID de la materia | Integer |
| resultado.fechaInscripcion | Fecha de inscripción | DateTime |
| resultado.estado | Estado de la inscripción | String |
| resultado.calificacion | Calificación obtenida | Decimal |
| resultado.fechaModificacion | Fecha de última modificación | DateTime |
| resultado.activo | Estado de activación | Boolean |
| resultado.estudianteNombre | Nombre del estudiante | String |
| resultado.materiaCodigoyNombre | Código y nombre de la materia | String |
| resultado.credMateria | Créditos de la materia | Integer |
| resultado.profesores | Lista de profesores asignados | Array |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Calificacion registrada",
  "detalle": "El estudiante Laura Gómez ha sido calificado con 4.5 en MAT101 - Matemáticas I",
  "resultado": {
    "id": 1,
    "estudianteId": 1,
    "materiaId": 1,
    "fechaInscripcion": "2024-01-15T10:00:00",
    "estado": "Aprobado",
    "calificacion": 4.5,
    "fechaModificacion": "2024-05-12T15:30:00",
    "activo": true,
    "estudianteNombre": "Laura Gómez",
    "materiaCodigoyNombre": "MAT101 - Matemáticas I",
    "credMateria": 3,
    "profesores": [
      "Juan García"
    ]
  }
}
```

#### 3.6.7. Cancelar Inscripción

Cancela una inscripción existente.

Acceso: `api/InscripcionEstudiante/{id}/cancelar`  
Formato: JSON  
Servicio: REST / PUT  
Autenticación: JWT requerido

##### 3.6.7.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID de la inscripción | Integer | Sí |

##### 3.6.7.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información de la inscripción actualizada | Object |
| resultado.id | Identificador único de la inscripción | Integer |
| resultado.estudianteId | ID del estudiante | Integer |
| resultado.materiaId | ID de la materia | Integer |
| resultado.fechaInscripcion | Fecha de inscripción | DateTime |
| resultado.estado | Estado de la inscripción | String |
| resultado.calificacion | Calificación obtenida | Decimal |
| resultado.fechaModificacion | Fecha de última modificación | DateTime |
| resultado.activo | Estado de activación | Boolean |
| resultado.estudianteNombre | Nombre del estudiante | String |
| resultado.materiaCodigoyNombre | Código y nombre de la materia | String |
| resultado.credMateria | Créditos de la materia | Integer |
| resultado.profesores | Lista de profesores asignados | Array |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Inscripcion cancelada",
  "detalle": "La inscripcion del estudiante Laura Gómez en MAT101 - Matemáticas I ha sido cancelada",
  "resultado": {
    "id": 1,
    "estudianteId": 1,
    "materiaId": 1,
    "fechaInscripcion": "2024-01-15T10:00:00",
    "estado": "Cancelado",
    "calificacion": null,
    "fechaModificacion": "2024-05-12T15:30:00",
    "activo": false,
    "estudianteNombre": "Laura Gómez",
    "materiaCodigoyNombre": "MAT101 - Matemáticas I",
    "credMateria": 3,
    "profesores": [
      "Juan García"
    ]
  }
}
```

## 4. SEGURIDAD

### 4.1. Autenticación

La API utiliza autenticación basada en tokens JWT (JSON Web Tokens). Para acceder a los endpoints protegidos, es necesario incluir el token en las cabeceras de la petición HTTP.

#### 4.1.1. Headers para Acceso a la API

Todos los endpoints requieren los siguientes headers básicos para el acceso a la API:

| **Header** | **Descripción** | **Ejemplo** |
|------------|-----------------|-------------|
| Sitio | Nombre del sitio | `StudentPortal` |
| Clave | Clave de acceso | `Portal123` |

#### 4.1.2. Headers para Autenticación JWT

Los endpoints protegidos requieren los siguientes headers adicionales:

| **Header** | **Descripción** | **Ejemplo** |
|------------|-----------------|-------------|
| Authorization | Token JWT con formato Bearer | `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...` |
| UsuarioId | ID del usuario (GUID) | `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` |

### 4.2. Gestión de Errores

La API devuelve respuestas estandarizadas para todos los endpoints. Los errores se manejan mediante códigos HTTP y mensajes descriptivos en el cuerpo de la respuesta.

#### 4.2.1. Códigos de Estado HTTP

| **Código** | **Descripción** |
|------------|-----------------|
| 200 | Éxito |
| 400 | Error en la solicitud (parámetros incorrectos) |
| 401 | Error de autenticación |
| 404 | Recurso no encontrado |
| 500 | Error interno del servidor |

#### 4.2.2. Estructura de Respuesta de Error

```json
{
  "exito": false,
  "mensaje": "Mensaje de error general",
  "detalle": "Descripción detallada del error",
  "resultado": null
}
```

### 4.3. Validaciones Principales

- Un estudiante no puede inscribir más de 3 materias simultáneamente.
- Un estudiante no puede tener clases con el mismo profesor en diferentes materias.
- Las calificaciones deben estar en el rango 0.0 - 5.0.
- La contraseña de un usuario debe tener al menos 6 caracteres, una letra mayúscula, una minúscula y un número.

## 5. EJEMPLOS DE USO

### 5.1. Flujo de Autenticación

#### 5.1.1. Login

Realizar una solicitud POST a `api/Auth/login` con las credenciales del usuario:

```json
// Solicitud
POST /api/Auth/login
Headers:
  Sitio: StudentPortal
  Clave: Portal123
  Content-Type: application/json

Body:
{
  "nombreUsuario": "admin",
  "contraseña": "AdminStudent2025*",
  "ip": "192.168.1.1"
}

// Respuesta
{
  "exito": true,
  "mensaje": "Usuario autenticado",
  "detalle": "El usuario admin se ha autenticado correctamente.",
  "resultado": {
    "usuario": {
      "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "nombreUsuario": "admin",
      "nombre": "Administrador",
      "apellido": "Sistema",
      "email": "admin@sistema.com",
      "rol": "Admin",
      "rolId": 1,
      "activo": true,
      "fechaCreacion": "2023-01-01T00:00:00",
      "fechaUltimoAcceso": "2024-05-12T12:30:45"
    },
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

#### 5.1.2. Uso del Token para Acceder a Endpoints Protegidos

```json
// Solicitud
GET /api/Estudiante
Headers:
  Sitio: StudentPortal
  Clave: Portal123
  Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
  UsuarioId: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
```

### 5.2. Creación y Gestión de Estudiante

#### 5.2.1. Registrar Usuario

```json
// Solicitud
POST /api/Auth/registro
Headers:
  Sitio: StudentPortal
  Clave: Portal123
  Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
  UsuarioId: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
  Content-Type: application/json

Body:
{
  "nombreUsuario": "usuario1",
  "contraseña": "Password123",
  "nombre": "Juan",
  "apellido": "Pérez",
  "email": "juan.perez@ejemplo.com",
  "rolId": 3
}
```

#### 5.2.2. Crear Estudiante

```json
// Solicitud
POST /api/Estudiante
Headers:
  Sitio: StudentPortal
  Clave: Portal123
  Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
  UsuarioId: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
  Content-Type: application/json

Body:
{
  "usuarioId": "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy",
  "identificacion": "1098765432",
  "telefono": "555-4321",
  "carrera": "Ingeniería de Sistemas",
  "programaId": 1,
  "activo": true
}
```

#### 5.2.3. Inscribir Estudiante en Materia

```json
// Solicitud
POST /api/InscripcionEstudiante
Headers:
  Sitio: StudentPortal
  Clave: Portal123
  Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
  UsuarioId: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
  Content-Type: application/json

Body:
{
  "estudianteId": 1,
  "materiaId": 1,
  "estado": "Inscrito",
  "activo": true
}
```

#### 5.2.4. Calificar Estudiante

```json
// Solicitud
PUT /api/InscripcionEstudiante/1/calificar?calificacion=4.5
Headers:
  Sitio: StudentPortal
  Clave: Portal123
  Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
  UsuarioId: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
```

## 6. CONSIDERACIONES ADICIONALES

### 6.1. Limitaciones

- Un estudiante puede inscribir un máximo de 3 materias simultáneamente.
- Un estudiante no puede tener clases con el mismo profesor en diferentes materias.
- Los estados válidos para una inscripción son: "Inscrito", "Aprobado", "Reprobado", "Cancelado".
- Las calificaciones deben estar en el rango 0.0 - 5.0.

### 6.2. Recomendaciones

- Almacenar el token JWT en un lugar seguro.
- Actualizar el token periódicamente.
- Validar los datos de entrada antes de enviarlos a la API.
- Manejar adecuadamente los errores devueltos por la API.

## 7. GLOSARIO (continuación)

- **Middleware**: Software que actúa como puente entre un sistema operativo o base de datos y las aplicaciones.
- **Entity Framework**: Framework de mapeo objeto-relacional para .NET que facilita el trabajo con bases de datos.
- **Trigger**: Procedimiento almacenado que se ejecuta automáticamente cuando ocurre un evento en una tabla de la base de datos.
- **Repository Pattern**: Patrón de diseño que aísla la capa de datos del resto de la aplicación.
- **HTTP Status Code**: Código numérico que indica el resultado de una solicitud HTTP.
- **SHA256**: Algoritmo de hash seguro que genera una firma digital de 256 bits.
- **Paginación**: Técnica para dividir grandes conjuntos de datos en "páginas" más pequeñas.
- **GUID**: Identificador único global, valor de 128 bits utilizado como identificador único.
- **Claim**: En JWT, información adicional sobre una entidad (generalmente el usuario) representada como pares clave-valor.

## 8. SOPORTE Y CONTACTO

### 8.1. Soporte Técnico

Para obtener soporte técnico relacionado con la API StudentPortal, contacte a través de los siguientes medios:

- **Email**: soporte@studentportal.com
- **Teléfono**: +1 (555) 123-4567
- **Portal de Soporte**: https://soporte.studentportal.com
- **Horario de Atención**: Lunes a Viernes, 9:00 AM - 6:00 PM (EST)

### 8.2. Reporte de Errores

Los errores o bugs encontrados en la API deben ser reportados a través del sistema de tickets, proporcionando la siguiente información:

1. Descripción detallada del error
2. Pasos para reproducir el error
3. Capturas de pantalla o logs relevantes
4. Información del entorno (sistema operativo, navegador, etc.)
5. Fecha y hora en que ocurrió el error

## 9. SOLUCIÓN DE PROBLEMAS COMUNES

### 9.1. Errores de Autenticación

- **Error 401 - Unauthorized**: Verifique que está incluyendo los headers correctos (Sitio, Clave, Authorization, UsuarioId).
- **Token Inválido**: Asegúrese de que el token JWT no ha expirado y que está correctamente formateado.
- **Credenciales Incorrectas**: Verifique el nombre de usuario y contraseña utilizados en el login.

### 9.2. Errores en Inscripciones

- **Error al Inscribir Estudiante**: Verifique que el estudiante no exceda el límite de 3 materias y que no tenga el mismo profesor en diferentes materias.
- **No se Puede Calificar**: Asegúrese de que la inscripción esté en estado "Inscrito" y que la calificación esté en el rango 0.0 - 5.0.
- **No se Puede Cancelar**: Verifique que la inscripción esté en estado "Inscrito" y sea activa.

### 9.3. Errores Generales

- **Error 500**: Este error indica un problema en el servidor. Contacte al soporte técnico proporcionando los detalles del error.
- **Error de Conexión**: Verifique su conexión a Internet y asegúrese de que la API esté disponible.
- **Error de Permisos**: Verifique que el usuario tenga los permisos necesarios para realizar la operación solicitada.

## 10. FUTURAS MEJORAS

La siguiente versión de la API StudentPortal incluirá las siguientes mejoras:

- Implementación de OAuth 2.0 para autenticación.
- Soporte para múltiples idiomas en las respuestas.
- Endpoint para carga masiva de estudiantes mediante archivo CSV.
- API para gestión de exámenes y calificaciones parciales.
- Mejoras en el rendimiento y optimización de consultas.
- Implementación de cache para respuestas frecuentes.
- Soporte para WebSockets para notificaciones en tiempo real.

## 11. APÉNDICES

### 11.1. Códigos de Error Específicos

| **Código** | **Descripción** | **Solución Recomendada** |
|------------|-----------------|--------------------------|
| E001 | Usuario no encontrado | Verifique el ID o nombre de usuario |
| E002 | Contraseña incorrecta | Asegúrese de escribir la contraseña correcta |
| E003 | Token expirado | Realice login nuevamente para obtener un nuevo token |
| E004 | Límite de materias excedido | Un estudiante solo puede inscribir 3 materias |
| E005 | Profesor duplicado | El estudiante ya tiene una materia con este profesor |
| E006 | Materia no disponible | La materia no está activa o no existe |
| E007 | Calificación fuera de rango | La calificación debe estar entre 0.0 y 5.0 |

### 11.2. Historial de Versiones

| **Versión** | **Fecha** | **Cambios Principales** |
|-------------|-----------|-------------------------|
| 1.0.0 | 15/01/2024 | Versión inicial |
| 1.1.0 | 01/03/2024 | Añadida funcionalidad de paginación |
| 1.2.0 | 15/04/2024 | Mejoras en la seguridad y validaciones |
| 1.3.0 | 01/05/2024 | Añadidos endpoints para gestión de programas |
| 1.4.0 | 12/05/2024 | Implementación de logging y mejoras de rendimiento |

## 12. CONCLUSIÓN

La API StudentPortal proporciona una solución completa para la gestión académica, permitiendo la administración de estudiantes, profesores, materias, programas e inscripciones de manera eficiente y segura. La implementación de estándares como JWT para la autenticación y REST para la arquitectura de la API facilita la integración con diferentes sistemas y aplicaciones cliente.

Se recomienda seguir las prácticas descritas en esta documentación para garantizar un uso óptimo de la API y prevenir posibles errores. Ante cualquier duda o problema, no dude en contactar al equipo de soporte técnico.

---

*Este documento fue actualizado por última vez el 12 de Mayo de 2024.*

*© 2025 StudentPortal. Todos los derechos reservados.*

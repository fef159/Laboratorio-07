/*
  Laboratorio 07 - BibliotecaDB
  SQL Server 2019 o posterior.
  El script es repetible: crea lo que no existe y solo inserta datos de prueba
  cuando las tablas correspondientes están vacías.
*/

IF DB_ID(N'BibliotecaDB') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE BibliotecaDB');
END;
GO

USE BibliotecaDB;
GO

IF OBJECT_ID(N'dbo.Autores', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Autores
    (
        AutorId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Autores PRIMARY KEY,
        Nombre       NVARCHAR(120) NOT NULL,
        Nacionalidad NVARCHAR(80) NOT NULL,
        Activo       BIT NOT NULL CONSTRAINT DF_Autores_Activo DEFAULT (1)
    );
END;
GO

IF OBJECT_ID(N'dbo.Libros', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Libros
    (
        LibroId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Libros PRIMARY KEY,
        Titulo     NVARCHAR(160) NOT NULL,
        ISBN       VARCHAR(20) NOT NULL,
        AutorId    INT NOT NULL,
        Ejemplares INT NOT NULL,
        Activo     BIT NOT NULL CONSTRAINT DF_Libros_Activo DEFAULT (1),
        CONSTRAINT UQ_Libros_ISBN UNIQUE (ISBN),
        CONSTRAINT CK_Libros_Ejemplares CHECK (Ejemplares >= 0),
        CONSTRAINT FK_Libros_Autores FOREIGN KEY (AutorId) REFERENCES dbo.Autores(AutorId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Socios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Socios
    (
        SocioId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Socios PRIMARY KEY,
        DNI     VARCHAR(12) NOT NULL,
        Nombre  NVARCHAR(120) NOT NULL,
        Email   NVARCHAR(160) NOT NULL,
        Activo  BIT NOT NULL CONSTRAINT DF_Socios_Activo DEFAULT (1),
        CONSTRAINT UQ_Socios_DNI UNIQUE (DNI)
    );
END;
GO

IF OBJECT_ID(N'dbo.Prestamos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Prestamos
    (
        PrestamoId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Prestamos PRIMARY KEY,
        SocioId      INT NOT NULL,
        FechaPrestamo DATE NOT NULL,
        FechaLimite   DATE NOT NULL,
        Estado        VARCHAR(12) NOT NULL CONSTRAINT DF_Prestamos_Estado DEFAULT ('Pendiente'),
        CONSTRAINT CK_Prestamos_Fechas CHECK (FechaLimite >= FechaPrestamo),
        CONSTRAINT CK_Prestamos_Estado CHECK (Estado IN ('Pendiente', 'Devuelto')),
        CONSTRAINT FK_Prestamos_Socios FOREIGN KEY (SocioId) REFERENCES dbo.Socios(SocioId)
    );
END;
GO

IF OBJECT_ID(N'dbo.DetallePrestamo', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DetallePrestamo
    (
        PrestamoId     INT NOT NULL,
        LibroId        INT NOT NULL,
        FechaDevolucion DATE NULL,
        CONSTRAINT PK_DetallePrestamo PRIMARY KEY (PrestamoId, LibroId),
        CONSTRAINT FK_DetallePrestamo_Prestamos FOREIGN KEY (PrestamoId) REFERENCES dbo.Prestamos(PrestamoId),
        CONSTRAINT FK_DetallePrestamo_Libros FOREIGN KEY (LibroId) REFERENCES dbo.Libros(LibroId)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Libros_Titulo' AND object_id = OBJECT_ID(N'dbo.Libros'))
    CREATE INDEX IX_Libros_Titulo ON dbo.Libros(Titulo);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Socios_Nombre' AND object_id = OBJECT_ID(N'dbo.Socios'))
    CREATE INDEX IX_Socios_Nombre ON dbo.Socios(Nombre);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Prestamos_FechaPrestamo' AND object_id = OBJECT_ID(N'dbo.Prestamos'))
    CREATE INDEX IX_Prestamos_FechaPrestamo ON dbo.Prestamos(FechaPrestamo);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Autores)
BEGIN
    INSERT INTO dbo.Autores (Nombre, Nacionalidad) VALUES
    (N'Gabriel García Márquez', N'Colombiana'),
    (N'Mario Vargas Llosa', N'Peruana'),
    (N'Isabel Allende', N'Chilena'),
    (N'Jorge Luis Borges', N'Argentina'),
    (N'Julio Cortázar', N'Argentina'),
    (N'Jane Austen', N'Británica'),
    (N'George Orwell', N'Británica'),
    (N'Antoine de Saint-Exupéry', N'Francesa');
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Libros)
BEGIN
    INSERT INTO dbo.Libros (Titulo, ISBN, AutorId, Ejemplares)
    SELECT v.Titulo, v.ISBN, a.AutorId, v.Ejemplares
    FROM (VALUES
      (N'Cien años de soledad',       '9780307474728', N'Gabriel García Márquez', 4),
      (N'El amor en los tiempos del cólera','9780307389732', N'Gabriel García Márquez', 3),
      (N'Crónica de una muerte anunciada','9781400034956', N'Gabriel García Márquez', 2),
      (N'La ciudad y los perros',      '9788420471839', N'Mario Vargas Llosa', 3),
      (N'Conversación en La Catedral', '9788420473109', N'Mario Vargas Llosa', 2),
      (N'La fiesta del Chivo',         '9788420474335', N'Mario Vargas Llosa', 3),
      (N'La casa de los espíritus',    '9788401352836', N'Isabel Allende', 4),
      (N'Paula',                       '9780061564901', N'Isabel Allende', 2),
      (N'El Aleph',                    '9788499089508', N'Jorge Luis Borges', 3),
      (N'Ficciones',                   '9788499089515', N'Jorge Luis Borges', 4),
      (N'Rayuela',                     '9788437604572', N'Julio Cortázar', 3),
      (N'Bestiario',                   '9788466331913', N'Julio Cortázar', 2),
      (N'Orgullo y prejuicio',         '9788491051329', N'Jane Austen', 5),
      (N'Sentido y sensibilidad',      '9788491051336', N'Jane Austen', 2),
      (N'1984',                        '9788499890944', N'George Orwell', 5),
      (N'Rebelión en la granja',       '9788499890951', N'George Orwell', 4),
      (N'Homenaje a Cataluña',         '9788499890968', N'George Orwell', 2),
      (N'El principito',               '9780156012195', N'Antoine de Saint-Exupéry', 6),
      (N'Vuelo nocturno',              '9788497597777', N'Antoine de Saint-Exupéry', 2),
      (N'Tierra de hombres',           '9788497597784', N'Antoine de Saint-Exupéry', 2)
    ) v(Titulo, ISBN, AutorNombre, Ejemplares)
    INNER JOIN dbo.Autores a ON a.Nombre = v.AutorNombre;
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Socios)
BEGIN
    INSERT INTO dbo.Socios (DNI, Nombre, Email) VALUES
    ('70123456', N'Ana Torres', 'ana.torres@example.com'),
    ('71234567', N'Luis Ramos', 'luis.ramos@example.com'),
    ('72345678', N'Carla Vega', 'carla.vega@example.com'),
    ('73456789', N'Diego Flores', 'diego.flores@example.com'),
    ('74567890', N'Elena Castillo', 'elena.castillo@example.com'),
    ('75678901', N'Fernando Ruiz', 'fernando.ruiz@example.com'),
    ('76789012', N'Gabriela Soto', 'gabriela.soto@example.com'),
    ('77890123', N'Hugo Mendoza', 'hugo.mendoza@example.com'),
    ('78901234', N'Inés Paredes', 'ines.paredes@example.com'),
    ('79012345', N'Jorge Salazar', 'jorge.salazar@example.com');
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Prestamos)
BEGIN
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;

    DECLARE @P1 INT, @P2 INT, @P3 INT, @P4 INT, @P5 INT;
    DECLARE @Hoy DATE = CONVERT(DATE, GETDATE());

    INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
    VALUES ((SELECT SocioId FROM dbo.Socios WHERE DNI='70123456'), DATEADD(DAY,-10,@Hoy), DATEADD(DAY,4,@Hoy), 'Pendiente');
    SET @P1 = SCOPE_IDENTITY();

    INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
    VALUES ((SELECT SocioId FROM dbo.Socios WHERE DNI='71234567'), DATEADD(DAY,-18,@Hoy), DATEADD(DAY,-4,@Hoy), 'Pendiente');
    SET @P2 = SCOPE_IDENTITY();

    INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
    VALUES ((SELECT SocioId FROM dbo.Socios WHERE DNI='72345678'), DATEADD(DAY,-30,@Hoy), DATEADD(DAY,-16,@Hoy), 'Devuelto');
    SET @P3 = SCOPE_IDENTITY();

    INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
    VALUES ((SELECT SocioId FROM dbo.Socios WHERE DNI='73456789'), DATEADD(DAY,-5,@Hoy), DATEADD(DAY,9,@Hoy), 'Pendiente');
    SET @P4 = SCOPE_IDENTITY();

    INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
    VALUES ((SELECT SocioId FROM dbo.Socios WHERE DNI='74567890'), DATEADD(DAY,-40,@Hoy), DATEADD(DAY,-26,@Hoy), 'Devuelto');
    SET @P5 = SCOPE_IDENTITY();

    -- Ana tiene exactamente tres libros pendientes para probar el límite.
    INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
    (@P1, (SELECT LibroId FROM dbo.Libros WHERE ISBN='9780307474728'), NULL),
    (@P1, (SELECT LibroId FROM dbo.Libros WHERE ISBN='9788420471839'), NULL),
    (@P1, (SELECT LibroId FROM dbo.Libros WHERE ISBN='9788401352836'), NULL),
    (@P2, (SELECT LibroId FROM dbo.Libros WHERE ISBN='9788499089508'), NULL),
    (@P3, (SELECT LibroId FROM dbo.Libros WHERE ISBN='9788437604572'), DATEADD(DAY,-18,@Hoy)),
    (@P3, (SELECT LibroId FROM dbo.Libros WHERE ISBN='9788491051329'), DATEADD(DAY,-17,@Hoy)),
    (@P4, (SELECT LibroId FROM dbo.Libros WHERE ISBN='9788499890944'), NULL),
    (@P5, (SELECT LibroId FROM dbo.Libros WHERE ISBN='9780156012195'), DATEADD(DAY,-25,@Hoy));

    UPDATE l SET Ejemplares = Ejemplares - x.Pendientes
    FROM dbo.Libros l
    INNER JOIN
    (
        SELECT LibroId, COUNT(*) Pendientes
        FROM dbo.DetallePrestamo
        WHERE FechaDevolucion IS NULL
        GROUP BY LibroId
    ) x ON x.LibroId = l.LibroId;

    COMMIT TRANSACTION;
END;
GO

SELECT
    (SELECT COUNT(*) FROM dbo.Autores) AS Autores,
    (SELECT COUNT(*) FROM dbo.Libros) AS Libros,
    (SELECT COUNT(*) FROM dbo.Socios) AS Socios,
    (SELECT COUNT(*) FROM dbo.Prestamos) AS Prestamos;
GO

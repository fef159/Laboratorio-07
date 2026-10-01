# Biblioteca WPF

Solución nueva para el Laboratorio 07, implementada con arquitectura en cuatro capas y acceso asíncrono a SQL Server.

## Proyectos

- `Biblioteca.Entidades`: entidades y objetos de resultado, sin acceso a datos ni reglas.
- `Biblioteca.Datos`: repositorios con consultas parametrizadas y transacciones.
- `Biblioteca.Negocio`: validaciones, límite de préstamos y cálculo de multa.
- `Biblioteca.WPF`: mantenimiento, préstamos, devoluciones y reporte.

## Preparación

1. Abra y ejecute `database/BibliotecaDB.sql` en SQL Server Management Studio.
2. La configuración incluida usa la instancia predeterminada local (`Server=.`). Si utiliza una instancia con otro nombre, cambie la cadena `BibliotecaDB` de `Biblioteca.WPF/App.config`.
3. Abra `Biblioteca.slnx` en Visual Studio 2022.
4. Establezca `Biblioteca.WPF` como proyecto de inicio y ejecute la aplicación.

## Decisión de acceso a datos

Se usan consultas directas parametrizadas. Esto mantiene las reglas funcionales en Negocio y permite revisar fácilmente que Datos no concatena valores en SQL. Las operaciones compuestas de préstamo y devolución sí se ejecutan dentro de transacciones SQL.

## Reglas implementadas

- ISBN y DNI únicos.
- Eliminación lógica de libros y socios.
- No se da de baja un libro o socio con préstamos pendientes.
- Un socio puede tener como máximo tres libros pendientes.
- No se prestan libros o socios inactivos ni libros sin stock.
- El préstamo registra cabecera, detalles y descuento de stock en una sola transacción.
- La devolución registra la fecha, repone stock y cambia el préstamo a `Devuelto` cuando corresponde.
- La multa se calcula en Negocio a razón de S/ 1.50 por día de retraso.

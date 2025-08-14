SELECT
    p.Apellido,
    p.Nombre,
    c.TipoCredito,
    c.observacion,
    c.MontoCredito,
    ISNULL(pagos.SumaPago, 0) AS SumaPago,
    (ISNULL(MAX(cu.ImporteCuota), 0) * c.NumCuotas) - ISNULL(pagos.SumaPago, 0) AS SaldoTotal
    
   
   

FROM Personas p
INNER JOIN Creditos c ON p.Id_Personas = c.Id_Personas
INNER JOIN Cuotas cu ON c.Id_Credito = cu.Id_Credito
LEFT JOIN (
    SELECT
        cu.Id_Credito,
        SUM(pa.MontoPagado) AS SumaPago
    FROM Cuotas cu
    LEFT JOIN Pagos pa ON cu.Id_Cuota = pa.Id_Cuota
    GROUP BY cu.Id_Credito
) pagos ON c.Id_Credito = pagos.Id_Credito

WHERE c.EstadoCredito = 'ACTIVO'
AND c.Id_Credito = @idcredito
GROUP BY
    p.Apellido,
    p.Nombre,
    c.TipoCredito,
    c.observacion,
    c.MontoCredito,
    c.NumCuotas,
    pagos.SumaPago,
    c.Id_Credito;


    -----------------------------------


    WITH CuotasOrdenadas AS (
    SELECT 
        cu.Id_Cuota,
        cu.FechaVto,
        cu.Estado,
        ROW_NUMBER() OVER (PARTITION BY cu.Id_Credito ORDER BY cu.FechaVto ASC) AS NumeroCuota
    FROM Cuotas cu
    WHERE cu.Id_Credito = @idcredito
     
)
SELECT 
    c.NumeroCuota,
    c.FechaVto,
    ISNULL(SUM(p.MontoPagado), 0) AS MontoPagado,
    c.Estado
FROM CuotasOrdenadas c
LEFT JOIN Pagos p ON c.Id_Cuota = p.Id_Cuota
GROUP BY c.NumeroCuota, c.FechaVto, c.Estado
ORDER BY c.NumeroCuota


select * from Personas
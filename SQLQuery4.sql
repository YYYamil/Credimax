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
                SELECT cu.Id_Credito, SUM(pa.MontoPagado) AS SumaPago
                FROM Cuotas cu
                LEFT JOIN Pagos pa ON cu.Id_Cuota = pa.Id_Cuota
                GROUP BY cu.Id_Credito
            ) pagos ON c.Id_Credito = pagos.Id_Credito
            WHERE c.EstadoCredito = 'ACTIVO' AND p.Id_Personas = '15'
            GROUP BY p.Apellido, p.Nombre, c.TipoCredito, c.observacion, c.MontoCredito, c.NumCuotas, pagos.SumaPago, c.Id_Credito
        
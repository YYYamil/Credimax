
SELECT Id_Credito, TipoCredito,MontoCredito,Observacion FROM Creditos WHERE Id_Personas = 44 AND EstadoCredito = 'ACTIVO'

SELECT 
    Id_Credito,
    TipoCredito,
    CASE 
        WHEN TipoCredito = 'Efectivo' THEN MontoCredito 
        ELSE NULL
    END AS MontoCredito,
    Observacion
FROM Creditos
WHERE Id_Personas = 44 AND EstadoCredito = 'ACTIVO';
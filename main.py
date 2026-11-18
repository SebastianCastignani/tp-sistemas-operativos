import threading

from avion import FIN
from buffer_acotado import BufferAcotado
from consumidor import Consumidor
from productor_api import ProductorApi


# Configuración simple del ejercicio.
CAPACIDAD_BUFFER = 4
CANTIDAD_CONSUMIDORES = 2
CANTIDAD_CONSULTAS = 1


def main() -> None:
    # Uno de los tres objetos compartidos es el buffer recibido por todos los hilos.
    buffer = BufferAcotado(CAPACIDAD_BUFFER)

    productor = threading.Thread(
        target=ProductorApi(
            "Buenos Aires",
            -34.6,
            -58.4,
            150,
            buffer,
            CANTIDAD_CONSULTAS,
        ).ejecutar,
        name="Productor-Buenos-Aires",
    )

    consumidores = []
    for numero in range(1, CANTIDAD_CONSUMIDORES + 1):
        consumidor = Consumidor(buffer)
        hilo = threading.Thread(
            target=consumidor.ejecutar,
            name=f"Consumidor-{numero}",
        )
        consumidores.append(hilo)

    # Los consumidores empiezan primero y pueden quedar bloqueados si el buffer está vacío.
    for consumidor in consumidores:
        consumidor.start()

    # Este productor consulta la API y deposita aviones mientras los consumidores trabajan.
    productor.start()

    # Join espera a que el productor termine todas sus consultas.
    productor.join()

    # El centinela FIN permite que cada consumidor salga de su ciclo.
    for consumidor in consumidores:
        buffer.poner(FIN)

    # Join espera a que los consumidores terminen después de recibir FIN.
    for consumidor in consumidores:
        consumidor.join()


if __name__ == "__main__":
    main()

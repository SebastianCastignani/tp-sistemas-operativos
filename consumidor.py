import time

from avion import Avion, FIN
from buffer_acotado import BufferAcotado


# Hilo consumidor que saca aviones del buffer y los muestra.
class Consumidor:
    def __init__(self, buffer: BufferAcotado, demora: float = 1):
        self.buffer = buffer
        self.demora = demora

    def ejecutar(self) -> None:
        while True:
            # sacar bloquea al consumidor si todavía no hay aviones disponibles.
            avion = self.buffer.sacar()

            # FIN es el único mensaje especial: permite terminar sin consumir datos reales.
            if avion is FIN:
                break

            altitud = str(avion.altitud_ft) if avion.altitud_ft is not None else "N/D"
            print(
                f"Avión {avion.icao} | Región: {avion.zona} "
                f"| Vuelo: {avion.callsign} | Altitud: {altitud} ft"
            )

            # Pausa breve para que los estados del buffer sean fáciles de observar.
            time.sleep(self.demora)

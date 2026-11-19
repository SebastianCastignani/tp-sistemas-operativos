from dataclasses import dataclass


# Datos de una lectura que viajan del productor al consumidor.
@dataclass
class Avion:
    # Cada campo describe un dato recibido o agregado por el programa.
    icao: str
    callsign: str
    altitud_ft: int | None
    lat: float
    lon: float
    zona: str
    secuencia: int


# Mensaje especial enviado una vez a cada consumidor para indicar su final.
FIN = Avion("", "", None, 0, 0, "", 0)

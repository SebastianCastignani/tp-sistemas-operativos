from dataclasses import dataclass


# Representa una lectura que viaja del productor al consumidor.
@dataclass
class Avion:
    icao: str
    callsign: str
    altitud_ft: int | None
    lat: float
    lon: float
    zona: str
    secuencia: int


# Mensaje especial que avisa a cada consumidor que debe terminar.
FIN = Avion("", "", None, 0, 0, "", 0)

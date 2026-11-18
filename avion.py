from dataclasses import dataclass
from datetime import datetime


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
    timestamp_utc: datetime


# Mensaje especial que avisa a cada consumidor que debe terminar.
FIN = Avion("", "", None, 0, 0, "", 0, datetime.now())

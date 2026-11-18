import json
import ssl
import urllib.request
import certifi

from avion import Avion
from buffer_acotado import BufferAcotado


# Hilo productor que consulta una zona y guarda las lecturas en el buffer.
class ProductorApi:
    def __init__(
        self,
        nombre: str,
        latitud: float,
        longitud: float,
        radio: int,
        buffer: BufferAcotado,
        cantidad_consultas: int = 1,
    ):
        self.nombre = nombre
        self.latitud = latitud
        self.longitud = longitud
        self.radio = radio
        self.buffer = buffer
        self.cantidad_consultas = cantidad_consultas
        self.secuencia = 0

    def ejecutar(self) -> None:
        for consulta in range(self.cantidad_consultas):
            try:
                url = (
                    "https://api.adsb.lol/v2/lat/"
                    f"{self.latitud}/lon/{self.longitud}/dist/{self.radio}"
                )
                solicitud = urllib.request.Request(url, headers={"User-Agent": "TP-Sistemas-Operativos"})

                contexto = ssl.create_default_context(cafile=certifi.where())
                with urllib.request.urlopen(solicitud, timeout=20, context=contexto) as respuesta:
                    datos = json.loads(respuesta.read().decode("utf-8"))

                for avion_json in datos.get("ac", []):
                    lat = avion_json.get("lat")
                    lon = avion_json.get("lon")
                    if lat is None or lon is None:
                        continue

                    altitud_json = avion_json.get("alt_baro")
                    if altitud_json == "ground":
                        altitud = 0
                    elif isinstance(altitud_json, (int, float)):
                        altitud = int(altitud_json)
                    else:
                        altitud = None

                    self.secuencia += 1
                    secuencia = self.secuencia

                    avion = Avion(
                        icao=str(avion_json.get("hex", "")),
                        callsign=str(avion_json.get("flight", "")).strip(),
                        altitud_ft=altitud,
                        lat=float(lat),
                        lon=float(lon),
                        zona=self.nombre,
                        secuencia=secuencia,
                    )

                    # Si el buffer está lleno, poner espera hasta que un consumidor libere lugar.
                    self.buffer.poner(avion)
                    print(f"[BUFFER] {self.nombre} puso el avión {avion.icao}.")

            except (OSError, json.JSONDecodeError) as error:
                print(f"[API/{self.nombre}] Error: {error}")

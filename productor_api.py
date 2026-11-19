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

                if not isinstance(datos, dict):
                    print(f"[API/{self.nombre}] Error: respuesta JSON con formato inesperado.")
                    continue

                aviones_json = datos.get("ac", [])
                if not isinstance(aviones_json, list):
                    print(f"[API/{self.nombre}] Error: la lista de aviones es inválida.")
                    continue

                for avion_json in aviones_json:
                    if not isinstance(avion_json, dict):
                        continue

                    lat = avion_json.get("lat")
                    lon = avion_json.get("lon")
                    if lat is None or lon is None:
                        continue

                    try:
                        altitud_json = avion_json.get("alt_baro")
                        if altitud_json == "ground":
                            altitud = 0
                        elif isinstance(altitud_json, (int, float)) and not isinstance(altitud_json, bool):
                            altitud = int(altitud_json)
                        else:
                            altitud = None

                        icao_json = avion_json.get("hex")
                        flight_json = avion_json.get("flight")
                        avion = Avion(
                            icao="" if icao_json is None else str(icao_json),
                            callsign="" if flight_json is None else str(flight_json).strip(),
                            altitud_ft=altitud,
                            lat=float(lat),
                            lon=float(lon),
                            zona=self.nombre,
                            secuencia=self.secuencia + 1,
                        )
                    except (TypeError, ValueError):
                        continue

                    self.secuencia = avion.secuencia

                    # Si el buffer está lleno, poner espera hasta que un consumidor libere lugar.
                    self.buffer.poner(avion)
                    print(f"[BUFFER] {self.nombre} puso el avión {avion.icao}.")

            except (OSError, json.JSONDecodeError, UnicodeDecodeError) as error:
                print(f"[API/{self.nombre}] Error: {error}")

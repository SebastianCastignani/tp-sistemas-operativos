# TP Sistemas Operativos - Productor/Consumidor en Python

Sistema sencillo de telemetría aeronáutica. Un productor consulta la API de
adsb.lol y dos consumidores leen las lecturas de un buffer compartido.

## Requisitos

- Python 3.10 o superior
- Se instala `certifi` para validar HTTPS

## Preparación en Linux

```bash
python3 -m pip install --upgrade pip
python3 -m pip install -r requirements.txt
```

## Ejecutar

```bash
python3 main.py
```

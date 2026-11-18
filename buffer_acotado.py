import threading

from avion import Avion


# Recurso compartido que guarda aviones entre el productor y los consumidores.
class BufferAcotado:
    def __init__(self, capacidad: int):
        self.datos = [None] * capacidad
        self.entrada = 0
        self.salida = 0

        # Un semáforo cuenta lugares libres y el otro cuenta aviones disponibles.
        self.lugares_libres = threading.Semaphore(capacidad)
        self.aviones_disponibles = threading.Semaphore(0)
        self.mutex = threading.Lock()

    def poner(self, avion: Avion) -> None:
        # Intentar adquirir primero permite avisar si el buffer está lleno.
        if not self.lugares_libres.acquire(blocking=False):
            print("[BUFFER LLENO] El productor espera hasta que haya un lugar.")
            self.lugares_libres.acquire()

        # Primero se espera el semáforo; si se bloqueara después de tomar el mutex,
        # los consumidores no podrían entrar al lock para sacar un lugar y habría deadlock.
        # SECCIÓN CRÍTICA: protege el arreglo circular y los índices entrada/salida.
        with self.mutex:
            self.datos[self.entrada] = avion
            self.entrada = (self.entrada + 1) % len(self.datos)

        # Release permite que un consumidor retire un avión.
        self.aviones_disponibles.release()

    def sacar(self) -> Avion:
        # Si no hay aviones, el consumidor queda bloqueado en este semáforo.
        self.aviones_disponibles.acquire()

        # Primero se espera el semáforo; si se esperara con el mutex tomado,
        # el productor no podría entrar al lock para liberar o agregar aviones.
        # SECCIÓN CRÍTICA: protege el arreglo circular y los índices entrada/salida.
        with self.mutex:
            avion = self.datos[self.salida]
            self.datos[self.salida] = None
            self.salida = (self.salida + 1) % len(self.datos)

        # Release deja disponible un lugar para el productor.
        self.lugares_libres.release()
        print("[BUFFER LIBERADO] Un consumidor sacó un avión.")
        return avion

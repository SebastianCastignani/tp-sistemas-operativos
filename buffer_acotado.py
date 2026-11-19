import threading

from avion import Avion


# Recurso compartido que guarda aviones entre el productor y los consumidores.
class BufferAcotado:
    def __init__(self, capacidad: int):
        # None representa una posición vacía del arreglo circular.
        self.datos = [None] * capacidad
        self.entrada = 0
        self.salida = 0

        # lugares_libres cuenta posiciones libres y aviones_disponibles cuenta datos listos.
        # Ambos semáforos permiten que un hilo espere sin consumir CPU.
        self.lugares_libres = threading.Semaphore(capacidad)
        self.aviones_disponibles = threading.Semaphore(0)
        # El mutex evita que dos hilos escriban o lean los índices al mismo tiempo.
        self.mutex = threading.Lock()

    def poner(self, avion: Avion) -> None:
        # Se prueba sin esperar para informar si el productor tendrá que bloquearse.
        if not self.lugares_libres.acquire(blocking=False):
            print("[BUFFER LLENO] El productor espera hasta que haya un lugar.")
            self.lugares_libres.acquire()

        # El semáforo se toma antes del mutex. Así el mutex nunca espera al semáforo.
        # SECCIÓN CRÍTICA: una escritura a la vez sobre el arreglo y el índice entrada.
        with self.mutex:
            self.datos[self.entrada] = avion
            self.entrada = (self.entrada + 1) % len(self.datos)

        # Avisa a un consumidor bloqueado que ya hay un dato disponible.
        self.aviones_disponibles.release()

    def sacar(self) -> Avion:
        # Si no hay aviones, la espera ocurre aquí y el consumidor no hace trabajo inútil.
        self.aviones_disponibles.acquire()

        # El semáforo se toma antes del mutex para mantener el orden seguro.
        # SECCIÓN CRÍTICA: una lectura a la vez sobre el arreglo y el índice salida.
        with self.mutex:
            avion = self.datos[self.salida]
            self.datos[self.salida] = None
            self.salida = (self.salida + 1) % len(self.datos)

        # Libera una posición; el productor puede usarla aunque otro consumidor siga activo.
        self.lugares_libres.release()
        print("[BUFFER LIBERADO] Un consumidor sacó un avión.")
        return avion

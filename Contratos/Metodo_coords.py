'''
Esto es una simulacion de lo que haria solidity para poder debuguear la parte de la interfaz
'''

class Terreno:
    
    def __init__(self, alto, largo, precio, owner):
        self.alto = alto
        self.largo = largo
        self.poseedores = dict()
        self.owner = owner
        self.precio = precio
        pass
    
    # FALTA -> Función para que el owner pueda sacar el dinero del contrato, o meter mas.
    
    
    # Funcion auxiliar que calcula el solapamiento entre dos rectangulos.
    # True si existe solapamiento, False en caso contrario
    # ((Xmin, Ymin),(Xmax, Ymax))
    def Area_Interseccion(self, a: tuple, b: tuple) -> bool:
        dx = min(a[1][0], b[1][0]) - max(a[0][0], b[0][0])
        dy = min(a[1][1], b[1][1]) - max(a[0][1], b[0][1])
        if (dx >= 0) and (dy >= 0):
            return True
        return False
    
    # Deberia de ser publico, para que la gente pueda ver que terrenos están libres y cuales no.
    def esTerrenoLibre(self, coords : tuple) -> bool:
        '''
        Devuelve True si la casilla está libre (no es de nadie). Falso en caso contrario
        '''
        for _ in self.poseedores.keys():
            for c in self.poseedores[_]:
                if self.Area_Interseccion(coords, c): # Si existe interseccion, entonces no es un área libre.
                    return False
        return True
    
    
    # Deberia de ser privado? Tipo, para que X persona no pueda cotillear en quien tiene qué
    def __esTerrenoAlquilado(self, poseedor : str, coords : tuple) -> bool:
        '''
        Devuelve True si el rectángulo pertenece a la persona indicada. Falso en caso contrario
        '''
        if poseedor not in self.poseedores.keys(): # El propietario ni existe
            return False
            
        if coords in self.poseedores[poseedor]: # El rectangulo  está en la pertenencia de alguien
            return True
        return False
        
        
    # Funcion para comprar un rectangulo de terreno.
    # 'nombre' deberia de ser la wallet que firma la operación
    def Apropia(self, nombre : str, coords_pixel : tuple) -> None:
        # ---------------- Comprobaciones de compra ----------------
        if nombre == self.owner:
            return None # Comprobar que quien desplega el contrato (propietario) no se puede comprar a si mismo los terrenos.
        
        if not self.esTerrenoLibre(coords_pixel):
            print("El terreno ya está ocupado.")
            return None # No se pudo efectuar la compra, el terreno ya es de alguien -> fin de la operación
        
        if (coords_pixel[0][0] > self.alto) or (coords_pixel[0][1] > self.largo) or (coords_pixel[0][0] < 0) or (coords_pixel[0][1] < 0) \
        or (coords_pixel[1][0] > self.alto) or (coords_pixel[1][1] > self.largo) or (coords_pixel[1][0] < 0) or (coords_pixel[1][1] < 0) \
        or (coords_pixel[0][0] >= coords_pixel[1][0]) or (coords_pixel[0][1] >= coords_pixel[1][1]): # Esto ultimo es para que las coords sean ((Xmin, Ymin),(Xmax, Ymax))
            print("Coordenadas inválidas.")
            return None # No se pudo efectuar la compra, coordenadas inválidas -> fin de la operación
        
        if (False): # nombre.dinero < precio (comprobar que el que solicita puede pagar el alquiler)
            return
        
        # ---------------- Registro de la Compra ----------------
        if nombre in self.poseedores.keys():
            self.poseedores[nombre].append(coords_pixel) # Si existia el propietario en la BD le pongo que compro el pixel
        else:
            self.poseedores[nombre] = [coords_pixel] # Si no, creo el propietario y registro su pixel
            
        pass
    
    
    # Funcion para devolver un rectangulo de terreno.
    # 'nombre' deberia de ser la wallet que firma la operación
    def Desapropia(self, nombre : str, coords_pixel : tuple) -> None:
        # ---------------- Comprobaciones de devolución ----------------
        if nombre == self.owner:
            return None # Comprobar que quien desplega el contrato (propietario) no se puede.
        
        if not self.__esTerrenoAlquilado(nombre, coords_pixel):
            print("El terreno no es de la persona buscada.")
            return None # No se pudo efectuar la devolución, el terreno no es de la persona indicada -> fin de la operación
        
        if ((coords_pixel[0][0] > self.alto) or (coords_pixel[0][1] > self.largo) or (coords_pixel[0][0] < 0) or (coords_pixel[0][1] < 0) \
        or (coords_pixel[1][0] > self.alto) or (coords_pixel[1][1] > self.largo) or (coords_pixel[1][0] < 0) or (coords_pixel[1][1] < 0) \
        or (coords_pixel[0][0] >= coords_pixel[1][0]) or (coords_pixel[0][1] >= coords_pixel[1][1])):
            return None
        
        
        # ---------------- Registro de la Devolución ----------------
        self.poseedores[nombre].remove(coords_pixel) # Si no, creo el propietario y registro su pixel
        if not self.poseedores[nombre]:
            del self.poseedores[nombre]
        pass
    
    # 'nombre' deberia de ser la wallet que firma la operación
    def Get_Posesiones(self, nombre : str) -> list:
        if nombre not in self.poseedores.keys():
            return []
        return self.poseedores[nombre]
    
    # Esto solo es de testeo de cara a Python
    def __str__(self) -> str:
        Dims = "Dimensiones del terreno: (" + str(self.alto) + "," + str(self.largo) + ")."
        Own = "\nPropietario: " + self.owner + '.'
        Poseedores = '\nPoseedores de terrenos:\n'
        for _ in self.poseedores.keys():
            Poseedores += '     > Nombre: ' + _ + ", Terrenos: " 
            for el in self.poseedores[_]:
                Poseedores += str(el) + ", "
            Poseedores += '\n'
        return Dims + Own + Poseedores



t = Terreno(2, 2, 0.3, "El caranchoa")
t.Apropia('Blanca', ((1, 1), (2, 2))) # -> OK
t.Apropia('Victor', ((0, 0), (0.9, 0.9))) # -> OK. Si pongo (1,1) no deja porque se pisan en el (1,1)
t.Apropia('Carlo', ((0, 0), (0, 0)))  # -> NO, ya es de victor
t.Apropia('Carlo', ((2, 2), (0, 0)))  # -> NO, se pasa de alto
t.Apropia('Carlo', ((1.1, 0), (1.9, 0.9)))  # -> OK
t.Apropia('Blanca', ((0, 1.1), (0.9, 1.9))) # -> OK

print("-----------")
print(t)
print("-----------")
t.Desapropia('Carlo', ((1.1, 0), (1.9, 0.9)))  # -> OK
t.Desapropia('Victor', ((1, 1), (2, 2))) # -> Nope, el terreno es de Blanca, no de Victor
print("-----------")
print(t)
print("-----------")
print("Las posesiones de Victor son: ", t.Get_Posesiones('Victor'))
print("Las posesiones de Carlo son: ", t.Get_Posesiones('Carlo'))
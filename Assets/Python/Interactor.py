
import socket
import numpy as np
import os.path
from Contratos.Metodo_pix import Terreno

host = 'localhost'
portLISTEN = 50000
portTALK = 50001
BUFFER_SIZE = 1024


print(""" 
'##::::'##:'########:'########::::'###::::'##::::'##:'########:'########:::'######:::'#######::
 ###::'###: ##.....::... ##..::::'## ##::: ##:::: ##: ##.....:: ##.... ##:'##... ##:'##.... ##:
 ####'####: ##:::::::::: ##:::::'##:. ##:: ##:::: ##: ##::::::: ##:::: ##: ##:::..:: ##:::: ##:
 ## ### ##: ######:::::: ##::::'##:::. ##: ##:::: ##: ######::: ########::. ######:: ##:::: ##:
 ##. #: ##: ##...::::::: ##:::: #########:. ##:: ##:: ##...:::: ##.. ##::::..... ##: ##:::: ##:
 ##:.:: ##: ##:::::::::: ##:::: ##.... ##::. ## ##::: ##::::::: ##::. ##::'##::: ##: ##:::: ##:
 ##:::: ##: ########:::: ##:::: ##:::: ##:::. ###:::: ########: ##:::. ##:. ######::. #######::
..:::::..::........:::::..:::::..:::::..:::::...:::::........::..:::::..:::......::::.......:::
""")

# DEBUG w/ PYTHON ONLY.
def Save_State(t: Terreno, filename: str) -> None:
    f = open(filename, 'w')
    f.write(str(t.alto) + '\n' + str(t.largo) + '\n' + t.owner + '\n' + str(t.precio) + '\n')

    import json
    f.write(json.dumps(t.poseedores))
    
    f.close()
    return None

# DEBUG w/ PYTHON ONLY.
def Load_State(filename: str) -> Terreno:
    import re
    import json
    
    f = open(filename, 'r')
    info = f.readlines()
    f.close()
    
    t = Terreno(int(info[0]), int(info[1]), re.findall("\d+\.\d+", info[3])[0], info[2].replace('\n', ''))
    registro = json.loads(info[4]) # Estan en listas en vez de tuplas
    for _ in registro.keys():
        t.poseedores[_] = []
        for el in registro[_]:
            t.poseedores[_].append((el[0], el[1]))
    
    return t

# DEBUG w/ PYTHON ONLY.
filename = 'ESTADO_ACTUAL.txt'
TERRENO = None


# Manda el mensaje al otro extremo de la comunicación
def Talk(MESSAGE) -> None:
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.connect((host, portTALK))
    s.send(MESSAGE.encode("utf-8"))
    s.close()



# ----------- TODO -------------
# DoAction > En vez de DONE se debe devolver;
  # BUY -> Mensaje de error si lo hay
  # Return -> Mensaje de erorr si lo hay
# DoAction > Poner otro check que devuelva todos los terrenos comprados.

# Unity > Despues de Load debería venir otro comando que sea el de recibir los terrenos ocupados, con un centinela o asi.
# O directamente en el LOAD que sea cuando se devuelvan tambien los terrenos ocupados y separado los del usuario del palo:
  # ancho, largo | ocupado1, ocupado2, ... | propio1, propio2, ...
  # https://learn.microsoft.com/en-us/dotnet/csharp/how-to/parse-strings-using-split
  # Int32.Parse(string_a_parsear)


def Return_Posesiones(data: str) -> str:
    '''
    Data -> string recibido desde C#
    Return -> Lista de pixeles comprados:- (x, y);(x2, y2);...;(xn, yn)
    '''
    posesiones = TERRENO.Get_Posesiones(data.split(' ')[1])
    info = ""
    for el in posesiones:
        info += '(' + str(el[0]) + "," + str(el[1]) + ');' 
    return info[:-1]


# Decodifica el mensaje y realiza la accion necesaria
def DoAction(data : str) -> str:
    data = data.decode("utf-8")
    
    # Consulta de estado # DEBUG w/ PYTHON ONLY >>> Consultar la blockchain y ver el último estado guardado
    global TERRENO
    if data == 'LOAD':
        if os.path.isfile(filename): 
            TERRENO = Load_State(filename)
        else:
            TERRENO = Terreno(10, 10, 1.0, "Pepe")
            
        return "!" + str(TERRENO).split('\n')[0].split(': ')[1].replace('.', '') # Manda el tamaño del terreno marcado con '!'
    
    # DEBUG w/ PYTHON ONLY
    if data == "SAVE":
        Save_State(TERRENO, filename)
        return "DONE"
    
    
    if "BUY" in data:  # BUY <Wallet> <Coords>
        Info = data.split(' ')
        A = int(Info[2].split(',')[0].replace('(', ''))
        B = int(Info[2].split(',')[1].replace(')', ''))
        TERRENO.Apropia(Info[1], (A, B))
        
        return Return_Posesiones(data)
    
    
    if "RETURN " in data:  # RETURN <Wallet> <Coords>
        Info = data.split(' ')
        A = int(Info[2].split(',')[0].replace('(', ''))
        B = int(Info[2].split(',')[1].replace(')', ''))
        TERRENO.Desapropia(Info[1], (A, B))
        
        return Return_Posesiones(data)
    
    
    if "CHECK" in data: # CHECK <Wallet>
        return Return_Posesiones(data)
    
    
    return "ERROR"
    

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.bind((host, portLISTEN))
s.listen(1)
conn, addr = s.accept()
print('Connection address:', addr)

while 1:
    data = conn.recv(BUFFER_SIZE)
    if not data: break
    print(data.decode('utf-8'))
    Action = DoAction(data).replace('\n', '')
    print("---------")
    print(TERRENO)
    print("---------")
    print("Python >>> C#: ", Action)
    Talk(Action)
conn.close()


# ---------------------------------------------------------+
# Cliente (C#) -> ¿Estado del terreno?                     |
# Servidor (Python) -> Consultar terreno                   |
# Blockchain (Solidity) -> Devuelve el estado              |
# Servidor (Python) -> Devuelve el estado                  |
# Cliente (C#) -> Representa el estado                     |
# ---------------------------------------------------------+
# ^ Este esquema se repite para todos los comandos posibles|
# ---------------------------------------------------------+
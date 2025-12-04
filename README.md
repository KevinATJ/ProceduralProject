# Generación Procedural de contenidos en Unity

## Creado por Kevin Troncoso, José Peña y Bastián Andrade

## Proyecto 1: Terrenos

## 1) Detalles del proyecto
Este proyecto implementa un sistema de generación procedural de terrenos en Unity.  
El objetivo de este es la posibilidad de generar dos mapas:
  1) Mapa normal: un mapa de carácter natural, que combina agua, playa, vegetación y montañas nevadas.
  2) Mapa volcánico: un mapa de carácter volcánico, que incluye terrenos de grandes montañas de lava y piedra volcánica.

- **Motor:** Unity 3D 2022.3.30f1
- **Lenguaje:** C#  
---

## 2) Algoritmos usados
1. **Diamond-Square (DS):**  
   Es el encargado de la generación del mapa de altura. 

2. **Cellular Automata (CA):**  
   Es el encargado de suavizar las transiciones entre arena, agua y tierra.

3. **L-System:**  
   Es el encargado de la generación de 3 tipos de árboles.

---

## 3) Instrucciones de uso en Unity
1. Clona el repositorio:  [Repositorio] (https://github.com/KevinATJ/ProceduralProject)
2. Descarga o ejecuta el proyecto en Unity 2022.3.30f1
3. Dirigete a la escena DSQ_PCG
4. Revisa Terrain, Tree y Manager para realizar cambios si lo deseas.
5. Ejecuta el proyecto.
6. Cambia y regenera mapas con los botones de tu teclado [1 y 2] y muevete en el mundo con [WASD Q E]

---

## 4) Instrucciones de uso en Ejecutable
1. Ejecuta My Project
2. Cambia y regenera mapas con los botones de tu teclado [1 y 2] y muevete en el mundo con [WASD Q E]
3. Presiona tecla [ESC] para modificar los algoritmos y el mapa.


## Proyecto 2: Puzzle

## 1) Detalles del proyecto
Este proyecto implementa un sistema procedural para desordenar y generar puzzles del tipo sliding puzzle.
El objetivo de este es permitir crear puzzles resolubles de distinta dificultad usando distintos tipos de algoritmos.

- **Motor:** Unity 2D 2022.3.30f1
- **Lenguaje:** C#  
---

## 2) Algoritmos usados
1. **Goal Backward:**
   Genera un puzzle resoluble mediante movimientos aleatorios pero válidos desde un estado inicial resuelto. cada iteración mueve la pieza vacía evitando repetir el último movimiento.
2. **Hill Climbing:**
    Busca maximizar la dificultad del puzzle usando distancia Manhattan como fitness. En cada iteración de este se generan vecinos válidos moviendo la pieza vacía y se toma el que mejore la dificultad.
3. **Algoritmo genético:**
   Genera puzzles mediante una población de estados que evolucionan usando una selección, crossover y mutación, mientras que tambien se asegura la solvibilidad con una correción de paridad.

---

## 3) Instrucciones de uso en Unity
1. Clona el repositorio:  [Repositorio] (https://github.com/KevinATJ/ProceduralProject/tree/Unit2)
2. Descarga o ejecuta el proyecto en Unity 2022.3.30f1
3. Dirigete a la escena Sl_Puzzle
4. Revisa las variables del PuzzleControler y cambialas si las deseas.
5. Ejecuta el proyecto.

---



## 4) Instrucciones de uso en Ejecutable
1. Ejecuta My Project
2. Presiona tecla [ESC] para modificar los algoritmos y las semillas.


---


## Proyecto 3: Mapas PCGML

Este proyecto presenta un sistema de generación de mapas en unity y en google colab.
El objetivo es permitir crear grandes variedades de mapas con distintos algoritmos, considerando etiquetarlos. 
Dentro de sus detalles se encuentran mapas formados por 4 tipos de tiles distintos:
  1) Pasto
  2) Casas
  3) Personas
  4) Agua


- **Motor:** Unity 2D 2022.3.30f1
- **Lenguaje:** C#  
---

## 2) Algoritmos usados
1. **WFC Complex:**
   Genera mapas nuevos a partir de patrones locales aprendidos del dataset. Usa propagación de restricciones para asegurar coherencia entre tiles y colapsa las celdas siguiendo reglas de compatibilidad
2. **Markov:**
    Genera mapas modelando probabilidades de transición entre tiles. Cada celda se elige según lo que suele aparecer junto a sus vecinas, creando variaciones rápidas y ligeras del estilo base.
3. **NN:**
   Predice la etiqueta del mapa (en este caso tipo de mapa) aprendiendo relaciones entre su estructura y la clase asignada. Permite automatizar el proceso de evaluación a partir del dataset entrenado.
3. **GAN:**
   Genera mapas completamente nuevos enfrentando dos redes, un generador que produce niveles y un discriminador que evalúa si se parecen al dataset real. Con el entrenamiento, crea estructuras originales y variadas.
   
---


## 3) Instrucciones de uso en Unity
1. Clona el repositorio:  [Repositorio] (https://github.com/KevinATJ/ProceduralProject/tree/Unit3)
2. Descarga o ejecuta el proyecto en Unity 2022.3.30f1
3. Dirigete a la escena Unit3
4. Revisa las variables del Map y cambialas si las deseas.
5. Ejecuta el proyecto.



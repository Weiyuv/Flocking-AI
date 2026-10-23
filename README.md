# Flocking AI
Bruno Chen Yongjian

Cena em Assets -> Scenes -> Flock

# Parâmetro
Peso da separação (separationWeight)	1.5

Peso do alinhamento (alignmentWeight)	1.0

Peso da coesão (cohesionWeight)	1.0

Raio de percepção (neighborRadius)	5.0

Raio de separação (separationRadius)	1.5

Velocidade máxima (maxSpeed)	5.0

Força máxima (maxForce)	8.0


# Experimentos com Boids

## Objetivo

Avaliar como os pesos de separação, alinhamento e o raio de percepção influenciam o comportamento coletivo dos boids, além de observar a reação do grupo à presença de um predador.

## Teste 1 — Peso da separação = 0

sem a força de separação, os boids deixam de evitar indivíduos muito próximos gerando sobreposição

## Teste 2 — Peso do alinhamento = 0

os boids deixam de sincronizar sua direção com a dos vizinhos,o movimento tende a ficar menos coordenado, mas a separação e a coesão ainda contribuem para evitar colisões e manter os indivíduos próximos.

## Teste 3 — Comparação do raio de percepção

### Raio = 0,5

 cada boid considera apenas vizinhos muito próximos, isso reduz a influência de indivíduos distantes e pode produzir agrupamentos menores 

### Raio = 5,0

os boids consideram uma região maior e podem responder a mais vizinhos, dependendo dos pesos utilizados, isso pode favorecer um movimento mais coordenado e uma formação coletiva mais ampla.

### Comparação teste 3

O raio de percepção altera a quantidade de vizinhos considerados no cálculo das forças, um raio menor privilegia interações locais, enquanto um raio maior permite que as decisões sejam influenciadas por uma parcela maior do grupo.

## Teste 4 — Predador e comportamento de fuga

os boids próximos ao predador tendem a se afastar, causando uma abertura ou dispersão local, a fuga se propagar para outros indivíduos deformando temporariamente a formação de grupos.

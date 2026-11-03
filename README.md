# Zadanie: Regulowany rynek

| Termin oddania | Punkty |
| --- | --- |
| 29.11.2026 23:00 | 20 pkt |

---

> **Kary za opóźnienia:**
> Przekroczenie terminu o **$n$** zajęć wiąże się z karą: uzyskana liczba punktów za realizację zadania jest dzielona przez **$2^n$**.

---

## 1. Opis problemu

Napisz aplikację w języku C#, która modeluje uproszczony system ekonomiczny przedstawiający interakcje na wolnym rynku ze sterowanym poziomem inflacji i podatków.

W symulowanym środowisku występują trzy główne podmioty (aktorzy):

### 1. Sprzedawcy

* Posiadają ograniczoną liczbę produktów wytwarzanych w jednostce czasu.
* Oferują produkty na wolnym rynku po cenach uzależnionych od:
* kosztu wytworzenia produktu,
* aktualnego poziomu inflacji i podatków,
* założonej marży (oczekiwanego zysku).


* Głównym celem sprzedawcy jest maksymalizacja zysku.

### 2. Kupujący

* Posiadają określone potrzeby, reguły decyzyjne oraz zasoby finansowe (zmieniające się w czasie).
* Śledzą oferty produktów na rynku według następujących zasad:
* Kupują produkty podzielone na dwie kategorie: **pierwszej potrzeby** oraz **luksusowe**. Produkty luksusowe nie muszą być kupowane natychmiast.
* Posiadają wiedzę o bieżącej skali inflacji.
* Ich skłonność do zakupu maleje wraz ze wzrostem ceny produktu — niezależnie od tego, czy wzrost cen wynika z inflacji, podatków czy podniesienia marży przez sprzedawcę.



### 3. Bank Centralny

* Obserwuje dynamikę wzrostu cen produktów oraz łączny obrót na rynku.
* Ustala bieżący poziom inflacji.
* Dąży do utrzymania stałych (kwotowo) wpływów podatkowych, wyliczanych jako iloczyn inflacji oraz obrotu rynkowego osiągniętego przy danej inflacji.

---

## 2. Wymagane wzorce projektowe

Projekt musi wykorzystywać dwa kluczowe wzorce projektowe do zarządzania komunikacją i aktualizacją stanu obiektów:

1. **Wzorzec Odwiedzający (Visitor):**
* Służy do modyfikacji i aktualizacji danych o produktach u sprzedawców oraz dostosowywania parametrów decyzyjnych kupujących.
* *Referencyjną implementację wzorca znajdziesz w katalogu:* `\Examples\ShoppingCart`.


2. **Wzorzec Obserwator (Observer):**
* Służy do pasywnego monitorowania zmian w systemie:
* **Sprzedawcy i Kupujący** obserwują **Bank Centralny**, aby na bieżąco reagować na zmiany poziomu inflacji.
* **Kupujący** obserwują oferty **Sprzedawców** i elastycznie reagują na pojawiające się okazje cenowe.


* *Referencyjną implementację wzorca znajdziesz w katalogu:* `\Examples\IObserverNet40`.


---

## 3. Wytyczne realizacyjne i architektoniczne

### Uwaga 1: Testy jednostkowe

Projekt musi zawierać kompletny zestaw testów jednostkowych (np. xUnit, NUnit lub MSTest) weryfikujących poprawność wyliczania cen, reakcji kupujących na inflację oraz działania zastosowanych wzorców projektowych.

### Uwaga 2: Architektura turowa

Dla uproszczenia implementacji i ułatwienia kontroli współbieżności zaleca się oparcie symulacji na modelu **turowym** (dyskretne kroki czasowe, w których kolejno następuje produkcja, publikacja ofert, decyzje zakupowe i korekta inflacji).

### Uwaga 3: Stabilność i odporność na zakłócenia

Zaimplementowany model rynku powinien osiągać stan równowagi i wykazywać odporność na gwałtowne zakłócenia (np. nagły skok marży sprzedawcy lub impuls inflacyjny).

> **Wymóg:** Zaimplementuj dedykowane testy jednostkowe sprawdzające stabilizację systemu oraz jego zachowanie w sytuacjach zakłóceń rynkowych.
# regulated-market-simulation

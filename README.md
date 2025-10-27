# SNUS Project - Causal + FIFO Broadcast System

## ?? Описание проекта

WCF приложение, реализующее систему из 4 сенсоров с **Causal + FIFO Broadcast** алгоритмом упорядочивания сообщений с использованием Vector Clock.

---

## ??? Архитектура

```
???????????????????
?  ProjectSNUS    ?  ? WCF Server (централизованный broadcast)
?  (WCF Service)  ?
???????????????????
         ?
         ???????????????????????????????????????????
      ?        ?    ?    ?
    ??????????   ??????????   ??????????   ??????????
    ?Sensor 0?   ?Sensor 1?   ?Sensor 2?   ?Sensor 3?
    ?Client1 ?   ?Client2 ?   ?Client3 ??Client4 ?
    ??????????   ????????????????????   ??????????
         ?       ?           ?    ?
         ???????????????????????????????????????????
  Causal Middleware Layer
```

---

## ?? Компоненты

### 1. **ProjectSNUS** (WCF Server)
- Регистрация/разрегистрация сенсоров
- Broadcast сообщений всем подключенным сенсорам
- Duplex communication через WSDualHttpBinding

### 2. **Client1-4** (Sensors)
- **CausalMiddleware.cs** - реализация Causal + FIFO broadcast
- **Vector Clock** - массив [4] для отслеживания причинности
- **Message Buffer** - буфер для сообщений, пришедших "не вовремя"
- **Windows Forms UI** - отправка и визуализация сообщений

---

## ?? Алгоритм Causal + FIFO Broadcast

### Vector Clock Rules:

**Отправка сообщения:**
```
VC[own_id]++
message.VectorClock = copy(VC)
```

**Проверка доставки (CanDeliver):**
```
1. FIFO: message.VC[sender] == local.VC[sender] + 1
2. Causal: ?i ? sender: message.VC[i] ? local.VC[i]
```

**Обновление после доставки:**
```
?i: local.VC[i] = max(local.VC[i], message.VC[i])
```

---

## ?? Как запустить

### ? Быстрый старт

**Вариант A (автоматически - PowerShell):**
```powershell
.\StartAll.ps1
```

**Вариант B (автоматически - Batch):**
```cmd
StartAll.bat
```

### ?? Ручной запуск

#### Шаг 1: Запустить WCF Server
```powershell
ProjectSNUS\bin\Debug\ProjectSNUS.exe
```
Или в Visual Studio: установите **ProjectSNUS** как Startup Project и нажмите **F5**

#### Шаг 2: Запустить 4 сенсора
```powershell
# Сенсор 0
Client1\bin\Debug\Client1.exe 0

# Сенсор 1  
Client1\bin\Debug\Client1.exe 1

# Сенсор 2
Client1\bin\Debug\Client1.exe 2

# Сенсор 3
Client1\bin\Debug\Client1.exe 3
```

**Примечание:** Все 4 сенсора используют один и тот же exe файл (Client1.exe), но с разными аргументами командной строки для SensorId.

### ?? Сборка проекта

Если нужно пересобрать:
```powershell
# Через MSBuild
msbuild ProjectSNUS.sln /p:Configuration=Debug

# Или через Visual Studio
Build -> Rebuild Solution
```

---

## ?? Тестирование

### Тест 1: FIFO порядок
1. Запустите все 4 сенсора
2. В Sensor 0 отправьте:
   - "Message 1"
   - "Message 2"
   - "Message 3"
3. **Ожидаемый результат:** Все сенсоры получат сообщения в порядке 1?2?3

### Тест 2: Causal dependency
1. Sensor 0 отправляет "A"
2. Sensor 1 (после получения "A") отправляет "B"
3. **Ожидаемый результат:** 
   - Все сенсоры получат "A" перед "B"
   - Vector Clock покажет причинную связь

### Тест 3: Concurrent messages
1. Sensor 0 отправляет "X"
2. Sensor 1 отправляет "Y" (одновременно)
3. **Ожидаемый результат:**
   - Сообщения могут быть доставлены в любом порядке
   - Но FIFO от каждого сенсора сохраняется

### Тест 4: Buffer visualization
1. Остановите Sensor 2 (закройте окно)
2. Sensor 0 отправляет несколько сообщений
3. Запустите Sensor 2 снова
4. **Ожидаемый результат:**
   - Sensor 2 догонит пропущенные сообщения
   - Buffer size покажет накопление/обработку

---

## ?? Что показывается в UI

Для каждого доставленного сообщения:
```
[DELIVERED from Sensor 1] Hello
Message VC: [1, 2, 0, 0]
Local VC: [1, 2, 0, 0]
Buffer size: 0
---
```

- **DELIVERED** - сообщение успешно доставлено после проверки
- **Message VC** - Vector Clock отправителя в момент отправки
- **Local VC** - текущий Vector Clock получателя
- **Buffer size** - количество сообщений в буфере

---

## ?? Структура файлов

```
SNUS-project/
??? ProjectSNUS/           # WCF Server
?   ??? IService1.cs          # Service contract
?   ??? Service1.cs         # Service implementation
?   ??? App.config       # WCF configuration
?
??? Client1/     # Sensor 0
?   ??? CausalMiddleware.cs   # Causal broadcast logic
?   ??? Form1.cs          # UI + WCF client
? ??? App.config
?
??? Client2/   # Sensor 1
??? Client3/           # Sensor 2
??? Client4/        # Sensor 3
?
??? ProjectSNUS.sln    # Solution file
```

---

## ?? Технические детали

- **Framework:** .NET Framework 4.7.2
- **WCF Binding:** WSDualHttpBinding (для duplex communication)
- **Vector Clock Size:** 4 (по количеству сенсоров)
- **Concurrency:** Thread-safe с использованием `lock`
- **UI Thread Safety:** InvokeRequired/Invoke pattern

---

## ?? Troubleshooting

### Проблема: "Unable to connect to service"
**Решение:** Убедитесь что ProjectSNUS запущен и слушает на правильном порту

### Проблема: "Address already in use"
**Решение:** Закройте все экземпляры сервера, проверьте порт 8733

### Проблема: Сообщения не доставляются
**Решение:** Проверьте:
1. Все сенсоры подключены к серверу
2. Vector Clock корректно обновляется
3. Buffer size в логе

### Проблема: Namespace errors при компиляции
**Решение:** 
```powershell
# Обновить Service Reference для каждого клиента
Update-ServiceReference ServiceReference1
```

---

## ?? Ссылки на теорию

- **Causal Broadcast:** Гарантирует причинный порядок событий
- **FIFO Broadcast:** Гарантирует порядок от каждого отправителя
- **Vector Clocks:** Lamport's vector timestamps для отслеживания причинности

---

## ? Checklist готовности проекта

- [x] WCF Server с duplex binding
- [x] 4 клиента-сенсора
- [x] CausalMiddleware с Vector Clock
- [x] FIFO + Causal ordering algorithm
- [x] Message Buffer с проверкой CanDeliver
- [x] UI для отправки и визуализации
- [x] Thread-safe реализация
- [x] Корректное обновление Vector Clock
- [x] Обработка отключения клиентов

---

## ????? Автор

SNUS Project - Distributed Systems Course

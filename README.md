# SNUS Project - Causal + FIFO Broadcast System

## ?? �������� �������

WCF ����������, ����������� ������� �� 4 �������� � **Causal + FIFO Broadcast** ���������� �������������� ��������� � �������������� Vector Clock.

---

## ??? �����������

```
???????????????????
?  ProjectSNUS    ?  ? WCF Server (���������������� broadcast)
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

## ?? ����������

### 1. **ProjectSNUS** (WCF Server)
- �����������/�������������� ��������
- Broadcast ��������� ���� ������������ ��������
- Duplex communication ����� WSDualHttpBinding

### 2. **Client1-4** (Sensors)
- **CausalMiddleware.cs** - ���������� Causal + FIFO broadcast
- **Vector Clock** - ������ [4] ��� ������������ �����������
- **Message Buffer** - ����� ��� ���������, ��������� "�� �������"
- **Windows Forms UI** - �������� � ������������ ���������

---

## ?? �������� Causal + FIFO Broadcast

### Vector Clock Rules:

**�������� ���������:**
```
VC[own_id]++
message.VectorClock = copy(VC)
```

**�������� �������� (CanDeliver):**
```
1. FIFO: message.VC[sender] == local.VC[sender] + 1
2. Causal: ?i ? sender: message.VC[i] ? local.VC[i]
```

**���������� ����� ��������:**
```
?i: local.VC[i] = max(local.VC[i], message.VC[i])
```

---

## ?? ��� ���������

### ? ������� �����

**������� A (������������� - PowerShell):**
```powershell
.\StartAll.ps1
```

**������� B (������������� - Batch):**
```cmd
StartAll.bat
```

### ?? ������ ������

#### ��� 1: ��������� WCF Server
```powershell
ProjectSNUS\bin\Debug\ProjectSNUS.exe
```
��� � Visual Studio: ���������� **ProjectSNUS** ��� Startup Project � ������� **F5**

#### ��� 2: ��������� 4 �������
```powershell
# ������ 0
Client1\bin\Debug\Client1.exe 0

# ������ 1  
Client1\bin\Debug\Client1.exe 1

# ������ 2
Client1\bin\Debug\Client1.exe 2

# ������ 3
Client1\bin\Debug\Client1.exe 3
```

**����������:** ��� 4 ������� ���������� ���� � ��� �� exe ���� (Client1.exe), �� � ������� ����������� ��������� ������ ��� SensorId.

### ?? ������ �������

���� ����� �����������:
```powershell
# ����� MSBuild
msbuild ProjectSNUS.sln /p:Configuration=Debug

# ��� ����� Visual Studio
Build -> Rebuild Solution
```

---

## ?? ������������

### ���� 1: FIFO �������
1. ��������� ��� 4 �������
2. � Sensor 0 ���������:
   - "Message 1"
   - "Message 2"
   - "Message 3"
3. **��������� ���������:** ��� ������� ������� ��������� � ������� 1?2?3

### ���� 2: Causal dependency
1. Sensor 0 ���������� "A"
2. Sensor 1 (����� ��������� "A") ���������� "B"
3. **��������� ���������:** 
   - ��� ������� ������� "A" ����� "B"
   - Vector Clock ������� ��������� �����

### ���� 3: Concurrent messages
1. Sensor 0 ���������� "X"
2. Sensor 1 ���������� "Y" (������������)
3. **��������� ���������:**
   - ��������� ����� ���� ���������� � ����� �������
   - �� FIFO �� ������� ������� �����������

### ���� 4: Buffer visualization
1. ���������� Sensor 2 (�������� ����)
2. Sensor 0 ���������� ��������� ���������
3. ��������� Sensor 2 �����
4. **��������� ���������:**
   - Sensor 2 ������� ����������� ���������
   - Buffer size ������� ����������/���������

---

## ?? ��� ������������ � UI

��� ������� ������������� ���������:
```
[DELIVERED from Sensor 1] Hello
Message VC: [1, 2, 0, 0]
Local VC: [1, 2, 0, 0]
Buffer size: 0
---
```

- **DELIVERED** - ��������� ������� ���������� ����� ��������
- **Message VC** - Vector Clock ����������� � ������ ��������
- **Local VC** - ������� Vector Clock ����������
- **Buffer size** - ���������� ��������� � ������

---

## ?? ��������� ������

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

## ?? ����������� ������

- **Framework:** .NET Framework 4.7.2
- **WCF Binding:** WSDualHttpBinding (��� duplex communication)
- **Vector Clock Size:** 4 (�� ���������� ��������)
- **Concurrency:** Thread-safe � �������������� `lock`
- **UI Thread Safety:** InvokeRequired/Invoke pattern

---

## ?? Troubleshooting

### ��������: "Unable to connect to service"
**�������:** ��������� ��� ProjectSNUS ������� � ������� �� ���������� �����

### ��������: "Address already in use"
**�������:** �������� ��� ���������� �������, ��������� ���� 8733

### ��������: ��������� �� ������������
**�������:** ���������:
1. ��� ������� ���������� � �������
2. Vector Clock ��������� �����������
3. Buffer size � ����

### ��������: Namespace errors ��� ����������
**�������:** 
```powershell
# �������� Service Reference ��� ������� �������
Update-ServiceReference ServiceReference1
```

---

## ?? ������ �� ������

- **Causal Broadcast:** ����������� ��������� ������� �������
- **FIFO Broadcast:** ����������� ������� �� ������� �����������
- **Vector Clocks:** Lamport's vector timestamps ��� ������������ �����������

---

## ? Checklist ���������� �������

- [x] WCF Server � duplex binding
- [x] 4 �������-�������
- [x] CausalMiddleware � Vector Clock
- [x] FIFO + Causal ordering algorithm
- [x] Message Buffer � ��������� CanDeliver
- [x] UI ��� �������� � ������������
- [x] Thread-safe ����������
- [x] ���������� ���������� Vector Clock
- [x] ��������� ���������� ��������

---

## ????? �����

SNUS Project - Distributed Systems Course

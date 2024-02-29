# MyMiniMart
 My mini mart

 Game đang có các tính năng như sau:
    - Bấm nút C để spawn các worker, shelfver, cashier, farmer.
    - Bấm nút Space để spawn customer
    - builidng đã dựng sẵn trong PROGRESS MANAGER.

 Config building editor:
    Mỗi Builiding sẽ có 3 thành phần chính config như sau:
    - Input Item(nguyên liệu): Thông thường chỉ có 1, có  thể có hơn 2 item, codeName là tên Item, slots Container sẽ chứa các vị trí của item đặt vào đấy.
    - Output Item(Sản phẩm): Cũng giống như Input, chỉ khác là sản phẩm sản xuất ra.
    - Action Slots(Action thực hiện với Building): Ví dụ, Input Action giành cho AI worker đến để nhập nguyên liệu vào, Output Action cho AI worker hoặc Customer lấy sản phẩm ra. NGoài ra còn có Process Action, Thief Action
    - Biến Locked để unlock building
    - ModelProcess: với những builing farm, ví dụ: animal chicken, đặt cà chua nguyên liệu vào, cà chua bay vào con gà rồi bay ra quả trứng, thì con gà là modelProcess.

 Config builing json:
    - Type:
        + 0: Cashier(Bàn tính tiền), 
        + 1: Shelf(Quầy đựng đồ),
        + 2: Farm(ví dụ animal chicken cần nguyên liệu mới sản xuất được),
        + 3: FarmAutoProcessing( ví dụ cây cà chua tự sản xuất)
    - Duration: Thời gian process
Config Unit Json
    - Type:
        + 0: Player
        + 1: Cashier, Worker, Farmer, Chef
        + 2: Customer
    - BaseMoveSpeed: tốc độ di chuyển
    - FollowRoutine: có follow routine hay không
    - Routine: Codename building + action type (với customer có thêm ItemCodeName và số lượng) 

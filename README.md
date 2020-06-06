# Covid-Check-Client

이 프로그램은 [여기서 볼 수 있는](https://github.com/SoftWareAndGuider/Covid-Check) Covid-Check의 클라이언트입니다.

이 프로젝트는 GPL에서 추가로 [이런](https://checks.trinets.xyz/rights) 추가적인 것이 적용됩니다.

### 클라이언트 프로그램이 하는 일
* 사용자 *관리
* 사용자 *체크
* 사용자 *발열 체크
* 사용자 *체크 해제

### 클라이언트 프로그램 개발시 필요한 것
* .Net Core SDK(3.1 이상)
* Nuget
   * Newtonsoft.Json 12.0.3
  
이 아래는 GUI프로그램 개발시 추가로 필요한 것
* GTK
* Nuget
   * GTKSharp 3.22.25.74

### 클라이언트 프로그램 실행시 필요한 것
* .Net Core Runtime x64(3.1 이상)
* 원활한 인터넷
* GTK(GUI 한정)

### 사용된 Nuget들
* GTKSharp 3.22.25.74 (LGPLv2) https://github.com/GtkSharp/GtkSharp
* Newtonsoft.Json 12.0.3 (MIT) https://github.com/JamesNK/Newtonsoft.Json


#### 여기서 사용하는 단어 설명
* 관리: 지정한 것을 생성, 삭제하는 것을 말합니다
* 체크: 지정한 것을 체크하지 않음 상태에서 체크 완료 상태로 바꾸는 것을 말합니다.
* 발열 체크: 지정한 것을 체크하지 않음 상태에서 발열이 확인됨 상태로 바꾸는 것을 말합니다.
* 체크 해제: 체크 완료 혹은 발열이 확인됨 상태에서 체크하지 않음 상태로 바꾸는 것을 말합니다.
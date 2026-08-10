# FancyZones Hotkeys (v2.0.0)

전역 단축키를 이용해 창 위치를 관리(FancyZones 방식)하는 네이티브 .NET 10 애플리케이션입니다.

## v2.0.0 주요 변경 사항
- Windows Defender의 오탐(멀웨어 오진) 문제를 해결하기 위해 기존 PowerShell 스크립트 기반에서 C# (.NET 10 WinForms)으로 완벽하게 재작성되었습니다.
- PowerToys FancyZones의 레이아웃 로직(`grid` 및 `canvas` 포맷, 여백 및 해상도 비율 포함)을 수학적으로 완벽하게 포팅하여 이식했습니다.
- 다중 모니터 환경을 지원하며 `primary`, `active`, `next`, `previous` 등 타겟 지정 기능을 제공합니다.
- 용량 최적화를 위해 자체 포함(Self-contained)된 .NET 런타임과 부분 트리밍(Trimming) 기술을 적용한 단일 실행 파일(`.exe`) 형태로 제공됩니다.
- 설정 파일(`presets.yaml`) 파싱에는 `YamlDotNet`을, PowerToys JSON 설정 파일 파싱에는 `System.Text.Json`을 사용합니다.
- 시스템 트레이 아이콘을 통해 백그라운드에서 조용히 실행됩니다.

## 빌드 방법
`build.ps1`을 실행하여 단일 실행 파일을 퍼블리시하고 Inno Setup (`setup.iss`)을 통해 패키징할 수 있습니다.

## 사용 방법
1. `presets.yaml` 파일을 수정하여 원하는 영역과 단축키를 정의합니다.
2. `FancyZonesHotkeys.exe`를 실행합니다.
3. 시스템 트레이에 아이콘이 나타납니다. 설정한 단축키를 사용하여 활성 창을 원하는 영역으로 스냅(이동)하세요!

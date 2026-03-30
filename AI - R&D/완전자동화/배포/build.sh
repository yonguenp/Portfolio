#!/usr/bin/env bash
# ============================================================
# Star Sweeper - Cocos Creator 3.8.8 Web Mobile 빌드 스크립트
# 용도: CocosCreator.exe CLI 헤드리스 빌드 자동화
#
# 사용법:
#   1. COCOS_CREATOR_PATH 환경변수 또는 스크립트 내 경로 수정 후 실행
#   2. bash build.sh
#
# 빌드 결과물 위치:
#   ../프로젝트/build/web-mobile/
# ============================================================

set -e

# -------- 설정 (환경에 맞게 수정) --------
PROJECT_PATH="$(cd "$(dirname "$0")/../프로젝트" && pwd)"
BUILD_PATH="${PROJECT_PATH}/build/web-mobile"

# CocosCreator 설치 경로 후보 (환경에 맞는 경로 주석 해제)
# COCOS_CREATOR_PATH="/c/Program Files/CocosCreator/Creator/3.8.8/CocosCreator.exe"
# COCOS_CREATOR_PATH="/c/CocosDashboard/editors/3.8.8/CocosCreator.exe"
# COCOS_CREATOR_PATH="${HOME}/CocosCreator/Creator/3.8.8/CocosCreator.exe"

if [ -z "${COCOS_CREATOR_PATH}" ]; then
    echo "[ERROR] COCOS_CREATOR_PATH 환경변수가 설정되지 않았습니다."
    echo "        CocosCreator.exe 경로를 환경변수로 지정하거나 스크립트 내 경로를 설정하세요."
    echo ""
    echo "  예시:  export COCOS_CREATOR_PATH='/c/CocosDashboard/editors/3.8.8/CocosCreator.exe'"
    echo "         bash build.sh"
    exit 1
fi

if [ ! -f "${COCOS_CREATOR_PATH}" ]; then
    echo "[ERROR] CocosCreator.exe 를 찾을 수 없습니다: ${COCOS_CREATOR_PATH}"
    exit 1
fi

echo "[BUILD] Star Sweeper - Web Mobile 빌드 시작"
echo "[BUILD] 프로젝트: ${PROJECT_PATH}"
echo "[BUILD] 출력: ${BUILD_PATH}"
echo ""

# -------- 빌드 실행 --------
"${COCOS_CREATOR_PATH}" \
    --project "${PROJECT_PATH}" \
    --build "platform=web-mobile;buildPath=${BUILD_PATH}" \
    --headless

BUILD_EXIT=$?

if [ ${BUILD_EXIT} -eq 0 ]; then
    echo ""
    echo "[BUILD] 빌드 성공!"
    echo "[BUILD] 결과물 위치: ${BUILD_PATH}"
    echo ""
    ls "${BUILD_PATH}" 2>/dev/null || echo "빌드 결과물 목록 조회 실패"

    # -------- gh-pages 배포 (선택) --------
    if command -v npx &>/dev/null; then
        echo ""
        echo "[DEPLOY] gh-pages 배포 시도..."
        cd "${PROJECT_PATH}"
        npx gh-pages -d "build/web-mobile" && echo "[DEPLOY] 배포 완료!" || echo "[DEPLOY] 배포 실패 (수동 배포 필요)"
    else
        echo "[DEPLOY] npx 없음 — gh-pages 배포 건너뜀"
    fi
else
    echo ""
    echo "[BUILD] 빌드 실패 (exit code: ${BUILD_EXIT})"
    echo "[BUILD] 에디터에서 수동 빌드: Build > Build & Preview > web-mobile"
    exit ${BUILD_EXIT}
fi

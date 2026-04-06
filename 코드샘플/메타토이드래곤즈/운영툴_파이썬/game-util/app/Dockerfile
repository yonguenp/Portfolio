# 1. 베이스 이미지 선택
FROM python:3.13-slim

# 2. 작업 디렉토리 설정
WORKDIR /app

# 3. 필요한 패키지 설치
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# 4. 애플리케이션 소스 코드 복사
COPY . .

# 5. Gunicorn으로 애플리케이션 실행 (55000번 포트 바인딩)
CMD ["gunicorn", "--bind", "0.0.0.0:55000", "app:app"]

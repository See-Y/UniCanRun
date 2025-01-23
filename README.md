# UniCanRun

## 팀원

UNIST 컴퓨터공학과 장영웅

SKKU 소프트웨어학과 장효진

## 개발환경

UNITY ML Agent package

mlagents  0.28.0

anaconda3

pytorch 1.11.0

## 넙죽이 참교육

그동안 넙죽이가 너무 많았다.

새로운 바람이 불어올 때가 되었다.

![윤이.jpg](윤이.jpg)

![넙죽이.jpeg](넙죽이.jpeg)

지겨워진 넙죽이를 잡기 위해 유니스트 마스코트 ‘윤이’를 데려온다.

## 유니콘 길들이기

우선 유니콘을 준비한다.

unity mlagent를 이용해 걷는 방법을 학습시킨다

## 강화학습

[몰입캠프 비디오  (1).mp4](몰입캠프 비디오 (1).mp4)

보상 함수 요약

1. 생존 보상 (매 스텝마다 작은 양수)
2. 균형 보상 (몸체가 수직에 가까울수록 높은 보상)
3. 방향 보상 (목표를 향해 바라볼수록 높은 보상)
4. 진행 보상 (목표 방향으로 빠르게 이동할수록 높은 보상)
5. 정지 페널티 (5초 이상 제자리에 있으면 큰 페널티)
6. 도달 보상 (목표에 도달하면 큰 보상)
7. 실패 페널티 (균형 상실, 높이 이탈, 거리 초과 시 페널티)

## 결과

[KillNupjuk.mp4](https://prod-files-secure.s3.us-west-2.amazonaws.com/f6cb388f-3934-47d6-9928-26d2e10eb0fc/94f0b809-f768-4916-8a0d-31fba34968a3/KillNupjuk.mp4)

---

## 추후에 도전하실 분들을 위한 팁

1. 패키지 버전 잘 맞춰야
    - 사용한 패키지 버전
        
        python 3.9.0
        
        absl-py                 2.1.0
        attrs                   24.2.0
        cachetools              5.5.0
        cattrs                  1.0.0
        certifi                 2022.12.7
        charset-normalizer      3.4.1
        cloudpickle             2.2.1
        google-auth             2.37.0
        google-auth-oauthlib    0.4.6
        grpcio                  1.62.3
        gym                     0.23.1
        gym-notices             0.0.8
        h5py                    3.8.0
        idna                    3.10
        importlib-metadata      4.13.0
        Markdown                3.4.4
        MarkupSafe              2.1.5
        mlagents                0.28.0
        mlagents-envs           0.28.0
        numpy                   1.21.2
        oauthlib                3.2.2
        Pillow                  9.5.0
        pip                     22.3.1
        protobuf                3.20.2
        pyasn1                  0.5.1
        pyasn1-modules          0.3.0
        pypiwin32               223
        pywin32                 308
        PyYAML                  6.0.1
        requests                2.31.0
        requests-oauthlib       2.0.0
        rsa                     4.9
        setuptools              65.6.3
        six                     1.17.0
        tensorboard             2.11.2
        tensorboard-data-server 0.6.1
        tensorboard-plugin-wit  1.8.1
        torch                   1.11.0
        torchaudio              0.11.0
        torchvision             0.12.0
        typing_extensions       4.7.1
        urllib3                 2.0.7
        Werkzeug                2.2.3
        wheel                   0.38.4
        wincertstore            0.2
        zipp                    3.15.0
        
2. Overfitting되는지 계속 살펴줘야
3. 이렇게 계속 버전 바뀌는 곳은 GPT를 절대 믿지 말고 unity docs를 봐라

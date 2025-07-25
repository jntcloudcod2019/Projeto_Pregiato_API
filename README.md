# 🧠 Projeto Pregiato API

**Pregiato API** é uma aplicação corporativa construída em ASP.NET Core 8, projetada para ser o **cérebro operacional de uma agência de modelos digital**. Essa API moderna centraliza o controle de cadastro, agendamentos, visualizações de portfólio, geração de contratos em PDF, autenticação de usuários e envio de notificações via WhatsApp e e-mail — tudo isso orquestrado com RabbitMQ e arquitetura limpa.

---

## 🎯 Objetivo do Projeto

Criar uma API robusta, escalável e extensível que permita a gestão completa de um ecossistema de agenciamento de modelos e eventos. O sistema é preparado para:

- Gestão de talentos e perfis
- Agendamento de **jobs**
- Emissão de contratos e relatórios
- Comunicação automatizada via WhatsApp e e-mail
- Operações assíncronas com mensagens em fila

Ideal para agências que querem elevar o nível da automação de seus processos internos.

---

## 🧱 Estrutura do Projeto
Projeto_Pregiato_API/
├── Controllers/           # Endpoints REST
├── DTOs/                  # Objetos de transferência (entrada/saída)
├── Enums/                 # Enumerações para status, tipo de usuário etc.
├── Helpers/               # Serviços auxiliares (JWT, Email, PDF, WhatsApp)
├── Middlewares/           # Interceptação de requisições/respostas
├── Models/                # Entidades do domínio
├── Repositories/          # Repositórios e interface de acesso a dados
├── Services/              # Regras de negócio encapsuladas
├── Migrations/            # Mapeamento do banco via EF Core
├── Program.cs             # Entry point e configuração
└── appsettings.json       # Configurações do ambiente


---

## 🛠️ Tecnologias Utilizadas

| Camada           | Tecnologia/Ferramenta                       |
|------------------|---------------------------------------------|
| Backend API      | ASP.NET Core 8                              |
| ORM              | Entity Framework Core                       |
| Banco de Dados   | PostgreSQL                                  |
| Fila de Mensagem | RabbitMQ                                    |
| PDF Automation   | PuppeteerSharp                              |
| Email            | MailKit + Templates HTML                    |
| Logs             | Serilog                                     |
| Autenticação     | JWT Bearer Token                            |
| Documentação     | Swagger (Swashbuckle)                       |
| Containers       | Docker + Docker Compose                     |

---

## 🧠 Padrões de Arquitetura e Técnicas Aplicadas

| Tipo                         | Detalhe                                                                 |
|------------------------------|-------------------------------------------------------------------------|
| **Arquitetura em Camadas**   | Separação entre API, serviço, domínio, repositório e utilitários       |
| **Repository Pattern**       | Encapsulamento de acesso a dados com interfaces                         |
| **DTO com AutoMapper**       | Mapeamento limpo entre modelo de domínio e dados expostos               |
| **Injeção de Dependência**   | Todos os serviços e repositórios são injetados via `IServiceCollection` |
| **Middleware Customizado**   | Manipulação global de erros, autenticação e validações                  |
| **Mensageria Assíncrona**    | Comunicação com consumidores via RabbitMQ (ex: envio de WhatsApp)       |
| **PDF dinâmico**             | Geração de contratos em HTML + PDF headless via PuppeteerSharp          |
| **JWT & Claims**             | Autorização segmentada por tipo de usuário                              |
| **Docker**                   | Ambiente totalmente containerizado com PostgreSQL, Redis e RabbitMQ     |

---

## ✅ Funcionalidades

- Cadastro e login de usuários com autenticação via JWT
- CRUD completo para modelos (incluindo JSONB para dados dinâmicos)
- Visualizações, propostas e agendamentos de trabalhos
- Geração de contratos PDF e templates de e-mail
- Disparo de mensagens via WhatsApp com RabbitMQ
- Upload de arquivos (como fotos e contratos)
- Logs estruturados com Serilog

---

## 🚀 Como Executar Localmente

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com)
- PostgreSQL local ou via Docker
- RabbitMQ (local ou Docker)
- Docker (opcional, mas recomendado)

Acesse via:
	•	http://localhost:5000/swagger — Documentação Swagger
	•	http://localhost:5000/api — API REST

⸻

📂 Exemplos de Endpoints
	•	POST /api/User/register — Registro de usuário
	•	POST /api/User/login — Login com JWT
	•	POST /api/Model — Cadastro de modelo
	•	GET /api/Job — Listar jobs disponíveis
	•	POST /api/Notification/send — Enviar WhatsApp via RabbitMQ

⸻

🧑‍💻 Autor

Desenvolvido com 💻 e ☕ por:

Jonathan – @jntcloudcod2019
Engenheiro de Software .NET | Automação Corporativa | IA + Cloud | Estratégia & Delivery

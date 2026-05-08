\encoding UTF8

\c crm_identity

DO $$
DECLARE
    admin_id uuid := '11111111-1111-1111-1111-111111111111';
    manager_id uuid := '22222222-2222-2222-2222-222222222222';
    employee_id uuid := '33333333-3333-3333-3333-333333333333';
    password_hash text := '100000.AQIDBAUGBwgJCgsMDQ4PEA==.bzEcqE9hRPQWn4FeJRSgVz55nPDelmY1pjJY8+IK56o=';
BEGIN
    INSERT INTO users ("Id", "Email", "PasswordHash", "FirstName", "LastName", "Role", "Status", "CreatedAt", "CreatedBy", "IsDeleted")
    VALUES
        (admin_id, 'admin@crm.local', password_hash, 'Анна', 'Администратор', 1, 1, now(), admin_id, false),
        (manager_id, 'manager@crm.local', password_hash, 'Михаил', 'Менеджер', 2, 1, now(), admin_id, false),
        (employee_id, 'employee@crm.local', password_hash, 'Елена', 'Сотрудник', 3, 1, now(), admin_id, false)
    ON CONFLICT ("Email") DO UPDATE SET
        "FirstName" = EXCLUDED."FirstName",
        "LastName" = EXCLUDED."LastName",
        "Role" = EXCLUDED."Role",
        "Status" = EXCLUDED."Status",
        "IsDeleted" = false;

    INSERT INTO refresh_tokens ("Id", "UserId", "Token", "ExpiresAt", "CreatedAt", "IpAddress", "UserAgent")
    VALUES
        ('11111111-aaaa-1111-aaaa-111111111111', admin_id, 'seed-admin-refresh-token', now() + interval '30 days', now(), '127.0.0.1', 'Seed script'),
        ('22222222-aaaa-2222-aaaa-222222222222', manager_id, 'seed-manager-refresh-token', now() + interval '30 days', now(), '127.0.0.1', 'Seed script')
    ON CONFLICT ("Token") DO UPDATE SET "ExpiresAt" = EXCLUDED."ExpiresAt", "RevokedAt" = NULL, "RevokedReason" = NULL;
END $$;

\c crm_clients

DO $$
DECLARE
    admin_id uuid := '11111111-1111-1111-1111-111111111111';
    manager_id uuid := '22222222-2222-2222-2222-222222222222';
    employee_id uuid := '33333333-3333-3333-3333-333333333333';
    client_alpha uuid := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1';
    client_beta uuid := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2';
    client_gamma uuid := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3';
    tag_vip uuid := 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1';
    tag_hot uuid := 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2';
    tag_enterprise uuid := 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3';
BEGIN
    INSERT INTO tags ("Id", "Name", "Description", "Color", "CreatedAt")
    VALUES
        (tag_vip, 'VIP', 'Ключевой клиент', '#8B5CF6', now()),
        (tag_hot, 'Горячий лид', 'Высокий приоритет', '#EF4444', now()),
        (tag_enterprise, 'Enterprise', 'Крупный бизнес', '#2563EB', now())
    ON CONFLICT ("Id") DO UPDATE SET
        "Name" = EXCLUDED."Name",
        "Description" = EXCLUDED."Description",
        "Color" = EXCLUDED."Color",
        "UpdatedAt" = now();

    INSERT INTO clients ("Id", "CompanyName", "Description", "AvatarUrl", "Inn", "Website", "Industry", "Status", "AssignedUserId", "CreatedAt", "CreatedBy", "IsDeleted")
    VALUES
        (client_alpha, 'ООО Альфа Софт', 'Разработка корпоративного ПО и интеграций', NULL, '7701234567', 'https://alpha-soft.example', 1, 2, manager_id, now() - interval '20 days', admin_id, false),
        (client_beta, 'АО Бета Финанс', 'Финансовые услуги для малого бизнеса', NULL, '7707654321', 'https://beta-finance.example', 2, 3, manager_id, now() - interval '14 days', admin_id, false),
        (client_gamma, 'Завод Гамма', 'Производство промышленного оборудования', NULL, '7801122334', 'https://gamma-factory.example', 3, 1, employee_id, now() - interval '7 days', manager_id, false)
    ON CONFLICT ("Id") DO UPDATE SET
        "CompanyName" = EXCLUDED."CompanyName",
        "Description" = EXCLUDED."Description",
        "Inn" = EXCLUDED."Inn",
        "Website" = EXCLUDED."Website",
        "Industry" = EXCLUDED."Industry",
        "Status" = EXCLUDED."Status",
        "AssignedUserId" = EXCLUDED."AssignedUserId",
        "IsDeleted" = false;

    INSERT INTO contacts ("Id", "ClientId", "FirstName", "LastName", "Email", "Phone", "Position", "IsPrimary", "CreatedAt", "CreatedBy")
    VALUES
        ('cccccccc-cccc-cccc-cccc-ccccccccccc1', client_alpha, 'Иван', 'Петров', 'ivan.petrov@alpha-soft.example', '+7 900 111-11-11', 'ИТ-директор', true, now(), manager_id),
        ('cccccccc-cccc-cccc-cccc-ccccccccccc2', client_alpha, 'Ольга', 'Смирнова', 'olga.smirnova@alpha-soft.example', '+7 900 111-11-12', 'Финансовый директор', false, now(), manager_id),
        ('cccccccc-cccc-cccc-cccc-ccccccccccc3', client_beta, 'Дмитрий', 'Кузнецов', 'd.kuznetsov@beta-finance.example', '+7 900 222-22-22', 'Руководитель продаж', true, now(), manager_id),
        ('cccccccc-cccc-cccc-cccc-ccccccccccc4', client_gamma, 'Мария', 'Соколова', 'm.sokolova@gamma-factory.example', '+7 900 333-33-33', 'Операционный директор', true, now(), employee_id)
    ON CONFLICT ("Id") DO UPDATE SET
        "ClientId" = EXCLUDED."ClientId",
        "FirstName" = EXCLUDED."FirstName",
        "LastName" = EXCLUDED."LastName",
        "Email" = EXCLUDED."Email",
        "Phone" = EXCLUDED."Phone",
        "Position" = EXCLUDED."Position",
        "IsPrimary" = EXCLUDED."IsPrimary";

    INSERT INTO client_tags ("ClientId", "TagsId")
    VALUES
        (client_alpha, tag_vip),
        (client_alpha, tag_enterprise),
        (client_beta, tag_hot),
        (client_gamma, tag_enterprise)
    ON CONFLICT DO NOTHING;
END $$;

\c crm_deals

DO $$
DECLARE
    admin_id uuid := '11111111-1111-1111-1111-111111111111';
    manager_id uuid := '22222222-2222-2222-2222-222222222222';
    employee_id uuid := '33333333-3333-3333-3333-333333333333';
    client_alpha uuid := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1';
    client_beta uuid := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2';
    client_gamma uuid := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3';
    refusal_price uuid := 'dddddddd-dddd-dddd-dddd-dddddddddd01';
    refusal_competitor uuid := 'dddddddd-dddd-dddd-dddd-dddddddddd02';
    deal_alpha uuid := 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1';
    deal_beta uuid := 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee2';
    deal_gamma uuid := 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee3';
    deal_lost uuid := 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee4';
BEGIN
    INSERT INTO refusal_reasons ("Id", "Name", "Description", "IsActive", "CreatedAt")
    VALUES
        (refusal_price, 'Цена', 'Клиент отказался из-за бюджета', true, now()),
        (refusal_competitor, 'Конкурент', 'Клиент выбрал конкурентное предложение', true, now())
    ON CONFLICT ("Id") DO UPDATE SET
        "Name" = EXCLUDED."Name",
        "Description" = EXCLUDED."Description",
        "IsActive" = true,
        "UpdatedAt" = now();

    INSERT INTO deals ("Id", "Title", "Description", "Amount", "Currency", "Stage", "Probability", "ClientId", "AssignedUserId", "ClosedAt", "RefusalReasonId", "CreatedAt", "CreatedBy", "IsDeleted")
    VALUES
        (deal_alpha, 'Внедрение CRM для Альфа Софт', 'Пилотный проект и интеграция с телефонией', 1450000.00, 1, 3, 50, client_alpha, manager_id, NULL, NULL, now() - interval '15 days', admin_id, false),
        (deal_beta, 'BI-отчётность для Бета Финанс', 'Дашборды руководителя и автоматизация отчётов', 920000.00, 1, 4, 75, client_beta, manager_id, NULL, NULL, now() - interval '10 days', manager_id, false),
        (deal_gamma, 'Сервисное обслуживание Завод Гамма', 'Годовой контракт на поддержку', 2500000.00, 1, 5, 100, client_gamma, employee_id, now() - interval '2 days', NULL, now() - interval '25 days', manager_id, false),
        (deal_lost, 'Модернизация портала Бета Финанс', 'Проект перенесён на следующий квартал', 680000.00, 1, 6, 0, client_beta, manager_id, now() - interval '1 days', refusal_price, now() - interval '18 days', manager_id, false)
    ON CONFLICT ("Id") DO UPDATE SET
        "Title" = EXCLUDED."Title",
        "Description" = EXCLUDED."Description",
        "Amount" = EXCLUDED."Amount",
        "Currency" = EXCLUDED."Currency",
        "Stage" = EXCLUDED."Stage",
        "Probability" = EXCLUDED."Probability",
        "ClientId" = EXCLUDED."ClientId",
        "AssignedUserId" = EXCLUDED."AssignedUserId",
        "ClosedAt" = EXCLUDED."ClosedAt",
        "RefusalReasonId" = EXCLUDED."RefusalReasonId",
        "IsDeleted" = false;

    INSERT INTO deal_stage_history ("Id", "DealId", "Stage", "ChangedAt", "ChangedByUserId")
    VALUES
        ('ffffffff-eeee-eeee-eeee-eeeeeeeeeee1', deal_alpha, 1, now() - interval '15 days', admin_id),
        ('ffffffff-eeee-eeee-eeee-eeeeeeeeeee2', deal_alpha, 2, now() - interval '11 days', manager_id),
        ('ffffffff-eeee-eeee-eeee-eeeeeeeeeee3', deal_alpha, 3, now() - interval '5 days', manager_id),
        ('ffffffff-eeee-eeee-eeee-eeeeeeeeeee4', deal_beta, 1, now() - interval '10 days', manager_id),
        ('ffffffff-eeee-eeee-eeee-eeeeeeeeeee5', deal_beta, 4, now() - interval '3 days', manager_id),
        ('ffffffff-eeee-eeee-eeee-eeeeeeeeeee6', deal_gamma, 5, now() - interval '2 days', employee_id),
        ('ffffffff-eeee-eeee-eeee-eeeeeeeeeee7', deal_lost, 6, now() - interval '1 days', manager_id)
    ON CONFLICT ("Id") DO UPDATE SET "DealId" = EXCLUDED."DealId", "Stage" = EXCLUDED."Stage", "ChangedAt" = EXCLUDED."ChangedAt", "ChangedByUserId" = EXCLUDED."ChangedByUserId";
END $$;

\c crm_tasks

DO $$
DECLARE
    admin_id uuid := '11111111-1111-1111-1111-111111111111';
    manager_id uuid := '22222222-2222-2222-2222-222222222222';
    employee_id uuid := '33333333-3333-3333-3333-333333333333';
    client_alpha uuid := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1';
    deal_alpha uuid := 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1';
    deal_beta uuid := 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee2';
    task_prepare uuid := '99999999-9999-9999-9999-999999999991';
    task_call uuid := '99999999-9999-9999-9999-999999999992';
    task_contract uuid := '99999999-9999-9999-9999-999999999993';
BEGIN
    INSERT INTO tasks ("Id", "Title", "Description", "DueDate", "Priority", "Status", "AssignedUserId", "RelatedEntityType", "RelatedEntityId", "CreatedAt", "CreatedBy", "IsDeleted")
    VALUES
        (task_prepare, 'Подготовить презентацию для Альфа Софт', 'Собрать кейсы, архитектуру и план внедрения', now() + interval '2 days', 3, 2, manager_id, 2, deal_alpha, now() - interval '4 days', manager_id, false),
        (task_call, 'Созвон с ИТ-директором Альфа Софт', 'Обсудить интеграции и сроки пилота', now() + interval '3 days', 2, 1, manager_id, 1, client_alpha, now() - interval '2 days', manager_id, false),
        (task_contract, 'Согласовать договор с Бета Финанс', 'Проверить финальную редакцию договора', now() + interval '5 days', 4, 1, employee_id, 2, deal_beta, now() - interval '1 days', manager_id, false)
    ON CONFLICT ("Id") DO UPDATE SET
        "Title" = EXCLUDED."Title",
        "Description" = EXCLUDED."Description",
        "DueDate" = EXCLUDED."DueDate",
        "Priority" = EXCLUDED."Priority",
        "Status" = EXCLUDED."Status",
        "AssignedUserId" = EXCLUDED."AssignedUserId",
        "RelatedEntityType" = EXCLUDED."RelatedEntityType",
        "RelatedEntityId" = EXCLUDED."RelatedEntityId",
        "IsDeleted" = false;

    INSERT INTO task_comments ("Id", "TaskId", "Content", "AuthorUserId", "CreatedAt")
    VALUES
        ('99999999-cccc-9999-cccc-999999999991', task_prepare, 'Добавить слайд про интеграцию с телефонией.', manager_id, now() - interval '1 days'),
        ('99999999-cccc-9999-cccc-999999999992', task_contract, 'Юристы клиента ждут финальный PDF.', employee_id, now() - interval '12 hours')
    ON CONFLICT ("Id") DO UPDATE SET "Content" = EXCLUDED."Content", "AuthorUserId" = EXCLUDED."AuthorUserId", "CreatedAt" = EXCLUDED."CreatedAt";

    INSERT INTO subtasks ("Id", "ParentTaskId", "Title", "Description", "DueDate", "Priority", "Status", "AssignedUserId", "Order", "CreatedAt", "CreatedBy", "IsDeleted")
    VALUES
        ('99999999-dddd-9999-dddd-999999999991', task_prepare, 'Собрать требования', 'Список интеграций и ограничений', now() + interval '1 days', 2, 3, manager_id, 0, now(), manager_id, false),
        ('99999999-dddd-9999-dddd-999999999992', task_prepare, 'Подготовить КП', 'Коммерческое предложение с этапами', now() + interval '2 days', 3, 1, manager_id, 1, now(), manager_id, false),
        ('99999999-dddd-9999-dddd-999999999993', task_contract, 'Проверить реквизиты', NULL, now() + interval '4 days', 2, 1, employee_id, 0, now(), manager_id, false)
    ON CONFLICT ("Id") DO UPDATE SET
        "ParentTaskId" = EXCLUDED."ParentTaskId",
        "Title" = EXCLUDED."Title",
        "Description" = EXCLUDED."Description",
        "DueDate" = EXCLUDED."DueDate",
        "Priority" = EXCLUDED."Priority",
        "Status" = EXCLUDED."Status",
        "AssignedUserId" = EXCLUDED."AssignedUserId",
        "Order" = EXCLUDED."Order",
        "IsDeleted" = false;

    INSERT INTO task_dependencies ("Id", "PredecessorTaskId", "SuccessorTaskId", "Type", "CreatedAt")
    VALUES
        ('99999999-eeee-9999-eeee-999999999991', task_prepare, task_call, 1, now()),
        ('99999999-eeee-9999-eeee-999999999992', task_call, task_contract, 2, now())
    ON CONFLICT ("Id") DO UPDATE SET "PredecessorTaskId" = EXCLUDED."PredecessorTaskId", "SuccessorTaskId" = EXCLUDED."SuccessorTaskId", "Type" = EXCLUDED."Type";
END $$;

\c crm_documents

DO $$
DECLARE
    manager_id uuid := '22222222-2222-2222-2222-222222222222';
    employee_id uuid := '33333333-3333-3333-3333-333333333333';
    deal_alpha uuid := 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1';
    task_contract uuid := '99999999-9999-9999-9999-999999999993';
BEGIN
    INSERT INTO documents ("Id", "FileName", "StoredFileName", "ContentType", "Size", "UploadedByUserId", "RelatedEntityType", "RelatedEntityId", "CreatedAt")
    VALUES
        ('12121212-1212-1212-1212-121212121211', 'presentation-alpha.pdf', 'seed/presentation-alpha.pdf', 'application/pdf', 245760, manager_id, 'Deal', deal_alpha, now()),
        ('12121212-1212-1212-1212-121212121212', 'contract-beta.docx', 'seed/contract-beta.docx', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document', 98304, employee_id, 'Task', task_contract, now())
    ON CONFLICT ("Id") DO UPDATE SET "FileName" = EXCLUDED."FileName", "StoredFileName" = EXCLUDED."StoredFileName", "ContentType" = EXCLUDED."ContentType", "Size" = EXCLUDED."Size", "RelatedEntityType" = EXCLUDED."RelatedEntityType", "RelatedEntityId" = EXCLUDED."RelatedEntityId";
END $$;

\c crm_notifications

DO $$
DECLARE
    manager_id uuid := '22222222-2222-2222-2222-222222222222';
    employee_id uuid := '33333333-3333-3333-3333-333333333333';
    task_contract uuid := '99999999-9999-9999-9999-999999999993';
    deal_alpha uuid := 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1';
BEGIN
    INSERT INTO notifications ("Id", "UserId", "Type", "Title", "Message", "Status", "RelatedEntityType", "RelatedEntityId", "CreatedAt")
    VALUES
        ('13131313-1313-1313-1313-131313131311', manager_id, 1, 'Новая задача', 'Подготовьте презентацию для Альфа Софт', 1, 'Deal', deal_alpha, now()),
        ('13131313-1313-1313-1313-131313131312', employee_id, 1, 'Срок задачи приближается', 'Проверьте договор с Бета Финанс', 1, 'Task', task_contract, now())
    ON CONFLICT ("Id") DO UPDATE SET "Title" = EXCLUDED."Title", "Message" = EXCLUDED."Message", "Status" = EXCLUDED."Status", "RelatedEntityType" = EXCLUDED."RelatedEntityType", "RelatedEntityId" = EXCLUDED."RelatedEntityId";
END $$;

\c crm_analytics

DO $$
DECLARE
    manager_id uuid := '22222222-2222-2222-2222-222222222222';
BEGIN
    INSERT INTO dashboard_widgets ("Id", "UserId", "WidgetType", "Title", "Config", "Position", "CreatedAt")
    VALUES
        ('14141414-1414-1414-1414-141414141411', manager_id, 'deals-pipeline', 'Воронка сделок', '{"period":"month"}', 1, now()),
        ('14141414-1414-1414-1414-141414141412', manager_id, 'tasks-summary', 'Задачи на неделю', '{"status":"open"}', 2, now())
    ON CONFLICT ("Id") DO UPDATE SET "WidgetType" = EXCLUDED."WidgetType", "Title" = EXCLUDED."Title", "Config" = EXCLUDED."Config", "Position" = EXCLUDED."Position";

    INSERT INTO reports ("Id", "Type", "Parameters", "GeneratedAt", "GeneratedByUserId", "Data")
    VALUES
        ('15151515-1515-1515-1515-151515151511', 1, '{"period":"month"}', now(), manager_id, '{"totalDeals":4,"won":1,"lost":1,"amount":5550000}'),
        ('15151515-1515-1515-1515-151515151512', 3, '{"period":"week"}', now(), manager_id, '{"openTasks":3,"completedSubTasks":1}')
    ON CONFLICT ("Id") DO UPDATE SET "Type" = EXCLUDED."Type", "Parameters" = EXCLUDED."Parameters", "GeneratedAt" = EXCLUDED."GeneratedAt", "Data" = EXCLUDED."Data";
END $$;

\c crm_audit

DO $$
DECLARE
    admin_id uuid := '11111111-1111-1111-1111-111111111111';
    manager_id uuid := '22222222-2222-2222-2222-222222222222';
    client_alpha uuid := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1';
    deal_alpha uuid := 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1';
BEGIN
    INSERT INTO audit_logs ("Id", "ServiceName", "EntityName", "EntityId", "Action", "UserId", "UserName", "Changes", "IpAddress", "CreatedAt")
    VALUES
        ('16161616-1616-1616-1616-161616161611', 'Crm.Clients', 'Client', client_alpha, 'Created', admin_id, 'Анна Администратор', '{"CompanyName":"ООО Альфа Софт"}', '127.0.0.1', now()),
        ('16161616-1616-1616-1616-161616161612', 'Crm.Deals', 'Deal', deal_alpha, 'Updated', manager_id, 'Михаил Менеджер', '{"Stage":"Переговоры"}', '127.0.0.1', now())
    ON CONFLICT ("Id") DO UPDATE SET "Action" = EXCLUDED."Action", "UserName" = EXCLUDED."UserName", "Changes" = EXCLUDED."Changes", "CreatedAt" = EXCLUDED."CreatedAt";
END $$;

-- Test users created by this script:
-- admin@crm.local / password: Test123!
-- manager@crm.local / password: Test123!
-- employee@crm.local / password: Test123!

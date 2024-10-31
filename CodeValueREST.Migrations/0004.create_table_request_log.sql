create table request_log (
    id             uuid        primary key,
    request_time   timestamp   not null,
    response_time  timestamp   not null,
    request_url    varchar(255) not null,
    request_size   integer     not null,
    response_code  integer     not null,
    response_size  integer     not null
);

comment on table request_log is 'Лог запросов';
comment on column request_log.id is 'Идентификатор';
comment on column request_log.timestamp is 'Время получения запроса';
comment on column request_log.response_time is 'Время ответа на запрос';
comment on column request_log.request_url is 'URL входящего запроса';
comment on column request_log.response_code is 'Код ответа';
comment on column request_log.request_size is 'Размер запроса в байтах';
comment on column request_log.response_size is 'Размер ответа в байтах';

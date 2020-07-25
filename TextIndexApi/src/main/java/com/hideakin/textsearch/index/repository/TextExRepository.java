package com.hideakin.textsearch.index.repository;

import java.util.List;

import com.hideakin.textsearch.index.entity.TextEntity;

public interface TextExRepository {

	List<TextEntity> findByTextContainingAndGid(String text, int gid, int limit, int offset);
	List<TextEntity> findByTextStartingWithAndGid(String text, int gid, int limit, int offset);
	List<TextEntity> findByTextEndingWithAndGid(String text, int gid, int limit, int offset);
	List<String> findTextByGid(int gid, int limit, int offset);
	List<TextEntity> findByGid(int gid, int limit, int offset);

}

package com.hideakin.textsearch.index.repository;

import java.util.List;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import com.hideakin.textsearch.index.entity.TextEntity;
import com.hideakin.textsearch.index.entity.TextId;

@Repository
public interface TextRepository extends JpaRepository<TextEntity,TextId> {

	TextEntity findByTextAndGid(String text, int gid);
	List<TextEntity> findAllByTextStartingWithAndGid(String text, int gid);
	List<TextEntity> findAllByTextEndingWithAndGid(String text, int gid);
	List<TextEntity> findAllByTextContainingAndGid(String text, int gid);
	void deleteByGid(int gid);

}

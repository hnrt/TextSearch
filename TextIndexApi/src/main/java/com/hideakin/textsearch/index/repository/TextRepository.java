package com.hideakin.textsearch.index.repository;

import javax.persistence.LockModeType;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Lock;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import com.hideakin.textsearch.index.entity.TextEntity;
import com.hideakin.textsearch.index.entity.TextId;

@Repository
public interface TextRepository extends JpaRepository<TextEntity,TextId> {

	TextEntity findByTextAndGid(String text, int gid);

	@Lock(LockModeType.PESSIMISTIC_WRITE)
	@Query("SELECT t FROM texts t WHERE t.text = :text AND t.gid = :gid")
	TextEntity findByTextAndGidForUpdate(@Param("text") String text, @Param("gid") int gid);

	void deleteByGid(int gid);

}

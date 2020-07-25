package com.hideakin.textsearch.index.repository;

import java.util.List;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;

import org.springframework.stereotype.Repository;

import com.hideakin.textsearch.index.entity.TextEntity;

@Repository
public class TextExRepositoryImpl implements TextExRepository {

	@PersistenceContext
	private EntityManager em;

	@Override
	public List<TextEntity> findByTextContainingAndGid(String text, int gid, int limit, int offset) {
		return findPartialByLike(String.format("%%%s%%", text), gid, limit, offset);
	}

	@Override
	public List<TextEntity> findByTextStartingWithAndGid(String text, int gid, int limit, int offset) {
		return findPartialByLike(String.format("%s%%", text), gid, limit, offset);
	}

	@Override
	public List<TextEntity> findByTextEndingWithAndGid(String text, int gid, int limit, int offset) {
		return findPartialByLike(String.format("%%%s", text), gid, limit, offset);
	}

	@Override
	public List<String> findTextByGid(int gid, int limit, int offset) {
		return em.createQuery("SELECT t.text FROM texts t WHERE t.gid = :gid ORDER BY t.text", String.class)
				.setParameter("gid", gid)
				.setMaxResults(limit)
				.setFirstResult(offset)
				.getResultList();
	}

	@Override
	public List<TextEntity> findByGid(int gid, int limit, int offset) {
		return em.createQuery("SELECT t FROM texts t WHERE t.gid = :gid ORDER BY t.text", TextEntity.class)
				.setParameter("gid", gid)
				.setMaxResults(limit)
				.setFirstResult(offset)
				.getResultList();
	}

	private List<TextEntity> findPartialByLike(String expr, int gid, int limit, int offset) {
		return em.createQuery("SELECT t FROM texts t WHERE t.text LIKE :expr AND t.gid = :gid ORDER BY t.text", TextEntity.class)
				.setParameter("expr", expr)
				.setParameter("gid", gid)
				.setMaxResults(limit)
				.setFirstResult(offset)
				.getResultList();
	}

}

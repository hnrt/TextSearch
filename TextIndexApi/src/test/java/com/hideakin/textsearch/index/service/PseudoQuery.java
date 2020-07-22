package com.hideakin.textsearch.index.service;

import java.util.Calendar;
import java.util.Date;
import java.util.List;
import java.util.Map;
import java.util.Set;

import javax.persistence.FlushModeType;
import javax.persistence.LockModeType;
import javax.persistence.Parameter;
import javax.persistence.Query;
import javax.persistence.TemporalType;

public class PseudoQuery implements Query {
	
	private Object value;
	
	public PseudoQuery(Object value) {
		this.value = value;
	}

	@SuppressWarnings("rawtypes")
	@Override
	public List getResultList() {
		return (List)value;
	}

	@Override
	public Object getSingleResult() {
		return value;
	}

	@Override
	public int executeUpdate() {
		return 0;
	}

	@Override
	public Query setMaxResults(int maxResult) {
		return this;
	}

	@Override
	public int getMaxResults() {
		return 0;
	}

	@Override
	public Query setFirstResult(int startPosition) {
		return this;
	}

	@Override
	public int getFirstResult() {
		return 0;
	}

	@Override
	public Query setHint(String hintName, Object value) {
		return this;
	}

	@Override
	public Map<String, Object> getHints() {
		return null;
	}

	@Override
	public <T> Query setParameter(Parameter<T> param, T value) {
		return this;
	}

	@Override
	public Query setParameter(Parameter<Calendar> param, Calendar value, TemporalType temporalType) {
		return this;
	}

	@Override
	public Query setParameter(Parameter<Date> param, Date value, TemporalType temporalType) {
		return this;
	}

	@Override
	public Query setParameter(String name, Object value) {
		return this;
	}

	@Override
	public Query setParameter(String name, Calendar value, TemporalType temporalType) {
		return this;
	}

	@Override
	public Query setParameter(String name, Date value, TemporalType temporalType) {
		return this;
	}

	@Override
	public Query setParameter(int position, Object value) {
		return this;
	}

	@Override
	public Query setParameter(int position, Calendar value, TemporalType temporalType) {
		return this;
	}

	@Override
	public Query setParameter(int position, Date value, TemporalType temporalType) {
		return this;
	}

	@Override
	public Set<Parameter<?>> getParameters() {
		return null;
	}

	@Override
	public Parameter<?> getParameter(String name) {
		return null;
	}

	@Override
	public <T> Parameter<T> getParameter(String name, Class<T> type) {
		return null;
	}

	@Override
	public Parameter<?> getParameter(int position) {
		return null;
	}

	@Override
	public <T> Parameter<T> getParameter(int position, Class<T> type) {
		return null;
	}

	@Override
	public boolean isBound(Parameter<?> param) {
		return false;
	}

	@Override
	public <T> T getParameterValue(Parameter<T> param) {
		return null;
	}

	@Override
	public Object getParameterValue(String name) {
		return null;
	}

	@Override
	public Object getParameterValue(int position) {
		return null;
	}

	@Override
	public Query setFlushMode(FlushModeType flushMode) {
		return this;
	}

	@Override
	public FlushModeType getFlushMode() {
		return null;
	}

	@Override
	public Query setLockMode(LockModeType lockMode) {
		return this;
	}

	@Override
	public LockModeType getLockMode() {
		return null;
	}

	@Override
	public <T> T unwrap(Class<T> cls) {
		return null;
	}

}
